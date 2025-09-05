using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;   // 👈 usamos el helper

namespace Ventapos.Api.Features;

public static class SalesEndpoints
{
    public static IEndpointRouteBuilder MapSales(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sales").WithTags("Ventas").RequireAuthorization();

        group.MapPost("/", async ([FromServices] Db db, [FromBody] SaleCreateRequest sale, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            // 👇 Una sola línea para preparar la auditoría
            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var uid = AuditoriaHelper.GetUserId(ctx) ?? 0;

            var caja = await conn.QueryFirstOrDefaultAsync<CajaApertura>(Sql.CashCurrent, transaction: tx);
            if (caja is null) return Results.Conflict(new { message = "No hay caja abierta" });

            decimal subtotal = 0, descuentoTotal = 0, granTotal = 0;
            foreach (var l in sale.Lineas)
            {
                var total = (l.PrecioUnitario * l.Cantidad) - l.Descuento;
                subtotal += l.PrecioUnitario * l.Cantidad;
                descuentoTotal += l.Descuento;
                granTotal += total;
            }

            var idVenta = await conn.ExecuteScalarAsync<long>(Sql.SaleHeadInsert, new
            {
                IdCliente = sale.IdCliente,
                IdUsuario = uid,
                IdCaja = caja.Id,
                Subtotal = subtotal,
                DescuentoTotal = descuentoTotal,
                GranTotal = granTotal
            }, tx);

            foreach (var l in sale.Lineas)
            {
                await conn.ExecuteAsync(Sql.SaleLineInsert, new
                {
                    IdVenta = idVenta,
                    IdCodigoBarra = l.IdCodigoBarra,
                    l.Cantidad,
                    l.PrecioUnitario,
                    l.Descuento,
                    Total = (l.PrecioUnitario * l.Cantidad) - l.Descuento
                }, tx);

                var barcode = await conn.QueryFirstOrDefaultAsync<CodigoBarra>(Sql.BarcodeGet, new { id = l.IdCodigoBarra }, tx);
                if (barcode is null) throw new Exception($"Código de barra {l.IdCodigoBarra} no encontrado");

                var tipo = await conn.QueryFirstOrDefaultAsync<TipoMovimiento>("SELECT * FROM tipos_movimiento WHERE nombre='VENTA' LIMIT 1", transaction: tx);
                if (tipo is null)
                {
                    var tipoId = await conn.ExecuteScalarAsync<long>(
                        "INSERT INTO tipos_movimiento(nombre,signo) VALUES('VENTA',-1); SELECT LAST_INSERT_ID();",
                        transaction: tx
                    );
                    tipo = new TipoMovimiento((int)tipoId, "VENTA", (sbyte)-1);
                }

                var cantidadReal = l.Cantidad * (barcode.Factor == 0 ? 1 : barcode.Factor);

                await conn.ExecuteAsync(Sql.MoveInsert, new
                {
                    IdProducto = barcode.Id_Producto,
                    IdTipoMovimiento = tipo.Id,
                    Cantidad = cantidadReal,
                    TipoOrigen = "venta",
                    IdOrigen = (int)idVenta,
                    IdUsuario = uid
                }, tx);

                var delta = -cantidadReal;
                await conn.ExecuteAsync(
                    "UPDATE productos SET stock_cache = COALESCE(stock_cache,0) + @delta WHERE id=@id",
                    new { id = barcode.Id_Producto, delta }, tx
                );
            }

            await tx.CommitAsync();
            return Results.Created($"/sales/{idVenta}", new { id = idVenta, subtotal, descuentoTotal, granTotal });
        });

        group.MapGet("/{id:int}", async ([FromServices] Db db, int id) =>
        {
            using var conn = await db.OpenAsync();
            var head = await conn.QueryFirstOrDefaultAsync<VentaHead>("SELECT * FROM ventas_head WHERE id=@id", new { id });
            if (head is null) return Results.NotFound();
            var lines = await conn.QueryAsync<DetalleVenta>("SELECT * FROM detalle_venta WHERE id_venta=@id", new { id });
            return Results.Ok(new { head, lines });
        });

        group.MapGet("/range", async ([FromServices] Db db, DateOnly from, DateOnly to) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync(@"
                SELECT * FROM ventas_head
                WHERE DATE(fecha) BETWEEN @from AND @to
                ORDER BY id DESC", new { from, to });
            return Results.Ok(rows);
        });

        return app;
    }
}
