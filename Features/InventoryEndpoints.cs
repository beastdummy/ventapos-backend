
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;

namespace Ventapos.Api.Features;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventory(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/inventory").WithTags("Inventario").RequireAuthorization();

        g.MapPost("/ajuste", async (AjusteStockRequest req, Db db, HttpContext ctx) => {
            if (req.Cantidad <= 0) return Results.BadRequest("Cantidad debe ser > 0");
            if (req.Signo != 1 && req.Signo != -1) return Results.BadRequest("Signo debe ser 1 o -1");

            using var conn = await db.OpenAsync();
            await AuditoriaHelper.PrepararAsync(conn, ctx);
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                var idMov = await conn.ExecuteScalarAsync<long>(InventorySql.InsertMovAlmacen, new {
                    req.IdSucursal, IdAlmacen = req.IdAlmacen, req.IdProducto, Tipo = "AJUSTE",
                    req.Cantidad, Signo = (int)req.Signo, TipoOrigen = "ajuste", IdOrigen = (long?)null,
                    IdUsuario = AuditoriaHelper.GetUserId(ctx), req.Observaciones
                }, tx);
                await conn.ExecuteAsync(InventorySql.AjusteStock, new {
                    IdAlmacen = req.IdAlmacen, IdProducto = req.IdProducto, Cantidad = req.Cantidad, Signo = (int)req.Signo
                }, tx);

                await tx.CommitAsync();
                return Results.Ok(new { id_movimiento = idMov });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Results.Problem(ex.Message);
            }
        });

        g.MapPost("/transfer", async (TransferRequest req, Db db, HttpContext ctx) => {
            if (req.Cantidad <= 0) return Results.BadRequest("Cantidad debe ser > 0");
            if (req.IdAlmacenOrigen == req.IdAlmacenDestino) return Results.BadRequest("Origen y destino no pueden ser iguales");

            using var conn = await db.OpenAsync();
            await AuditoriaHelper.PrepararAsync(conn, ctx);
            using var tx = await conn.BeginTransactionAsync();

            try
            {
                var movOut = await conn.ExecuteScalarAsync<long>(InventorySql.InsertMovAlmacen, new {
                    req.IdSucursal, IdAlmacen = req.IdAlmacenOrigen, req.IdProducto, Tipo = "TRANSFERENCIA",
                    Cantidad = req.Cantidad, Signo = -1, TipoOrigen = "transferencia", IdOrigen = (long?)null,
                    IdUsuario = AuditoriaHelper.GetUserId(ctx), Observaciones = req.Observaciones
                }, tx);
                var movIn = await conn.ExecuteScalarAsync<long>(InventorySql.InsertMovAlmacen, new {
                    req.IdSucursal, IdAlmacen = req.IdAlmacenDestino, req.IdProducto, Tipo = "TRANSFERENCIA",
                    Cantidad = req.Cantidad, Signo = 1, TipoOrigen = "transferencia", IdOrigen = (long?)null,
                    IdUsuario = AuditoriaHelper.GetUserId(ctx), Observaciones = req.Observaciones
                }, tx);

                await conn.ExecuteAsync(InventorySql.TransferStock, new {
                    req.IdProducto, req.Cantidad, req.IdAlmacenOrigen, req.IdAlmacenDestino
                }, tx);

                await tx.CommitAsync();
                return Results.Ok(new { mov_salida = movOut, mov_entrada = movIn });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Results.Problem(ex.Message);
            }
        });

        g.MapGet("/movimientos/product/{productId:int}", async (int productId, Db db) => {
            using var conn = await db.OpenAsync();
            var list = await conn.QueryAsync(InventorySql.MovsByProduct, new { IdProducto = productId });
            return Results.Ok(list);
        });

        return g;
    }
}
