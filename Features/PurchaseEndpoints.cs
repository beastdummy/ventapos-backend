using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;
using Ventapos.Api.Utils; // AuditoriaHelper

namespace Ventapos.Api.Features;

public static class PurchaseEndpoints
{
    public static IEndpointRouteBuilder MapPurchases(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/compras")
                       .WithTags("Compras")
                       .RequireAuthorization();

        // Crear compra (cabecera + líneas) con auditoría y transacción
        group.MapPost("/", async (
            [FromServices] Db db,
            [FromBody] PurchaseCreateRequest req,
            HttpContext ctx) =>
        {
            if (req.Lineas is null || req.Lineas.Count == 0)
                return Results.BadRequest(new { message = "Debe enviar al menos una línea." });

            // Validaciones básicas
            foreach (var l in req.Lineas)
            {
                if (l.Cantidad <= 0 || l.CostoUnitario < 0)
                    return Results.BadRequest(new { message = "Cantidad debe ser > 0 y costo >= 0." });
            }

            var uid = GetUserId(ctx); // puede ser null si no mapeas el sub a int

            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            // Auditoría: setear variables de contexto
            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            // Insert cabecera
            var compraId = await conn.ExecuteScalarAsync<long>(
                Sql.PurchaseHeadInsert,
                new
                {
                    IdProveedor = req.IdProveedor,       // puede ser null
                    IdUsuario = uid,                     // null si no se pudo leer del token
                    NroDoc = string.IsNullOrWhiteSpace(req.NroDoc) ? null : req.NroDoc
                },
                tx
            );

            // Insert líneas
            foreach (var l in req.Lineas)
            {
                var totalLinea = (l.Cantidad * l.CostoUnitario) - l.Descuento;
                await conn.ExecuteAsync(
                    Sql.PurchaseDetailInsert,
                    new
                    {
                        IdCompra = compraId,
                        IdProducto = l.IdProducto,
                        IdCodigoBarra = l.IdCodigoBarra,   // puede ser null
                        Descripcion = l.Descripcion,
                        Cantidad = l.Cantidad,
                        CostoUnitario = l.CostoUnitario,
                        Descuento = l.Descuento,
                        TotalLinea = totalLinea
                    },
                    tx
                );
            }

            // Recalcular totales en cabecera
            await conn.ExecuteAsync(Sql.PurchaseRecalcTotals, new { IdCompra = compraId }, tx);

            // Aplicar al stock (sumar entradas)
            await conn.ExecuteAsync(Sql.PurchaseApplyStock, new { IdCompra = compraId }, tx);

            await tx.CommitAsync();

            return Results.Created($"/compras/{compraId}", new { id = compraId });
        });

        // Obtener compra por id (cabecera + líneas)
        group.MapGet("/{id:long}", async ([FromServices] Db db, long id) =>
        {
            using var conn = await db.OpenAsync();
            var head = await conn.QueryFirstOrDefaultAsync(Sql.PurchaseGet, new { id });
            if (head is null) return Results.NotFound();

            var lines = await conn.QueryAsync(Sql.PurchaseLinesByCompra, new { id });
            return Results.Ok(new { compra = head, lineas = lines });
        });

        return app;
    }

    private static int? GetUserId(HttpContext ctx)
    {
        var c = ctx.User?.FindFirst(ClaimTypes.NameIdentifier) ?? ctx.User?.FindFirst("sub");
        return int.TryParse(c?.Value, out var id) ? id : null;
    }
}

// ===== DTOs =====
public sealed record PurchaseCreateRequest(
    int? IdProveedor,
    string? NroDoc,
    List<PurchaseLineDto> Lineas
);

public sealed record PurchaseLineDto(
    int IdProducto,
    int? IdCodigoBarra,
    string? Descripcion,
    int Cantidad,
    decimal CostoUnitario,
    decimal Descuento
);
