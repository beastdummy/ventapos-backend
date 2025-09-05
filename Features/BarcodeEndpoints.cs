using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;   // 👈 importa el helper

namespace Ventapos.Api.Features;

public static class BarcodeEndpoints
{
    public static IEndpointRouteBuilder MapBarcodes(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/barcodes").WithTags("Códigos de barra").RequireAuthorization();

        group.MapGet("/by-product/{productId:int}", async ([FromServices] Db db, int productId) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync<CodigoBarra>(Sql.BarcodeByProduct, new { productId });
            return Results.Ok(rows);
        });

        group.MapPost("/", async ([FromServices] Db db, [FromBody] BarcodeUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx); // 👈 auditoría en una línea

            var id = await conn.ExecuteScalarAsync<long>(Sql.BarcodeInsert, new
            {
                IdProducto = body.IdProducto,
                Codigo = body.Codigo,
                Factor = body.Factor,
                Descripcion = body.Descripcion,
                IdProveedor = body.IdProveedor
            }, transaction: tx);

            await tx.CommitAsync();
            return Results.Created($"/barcodes/{id}", new { id });
        });

        group.MapPut("/{id:int}", async ([FromServices] Db db, int id, [FromBody] BarcodeUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(Sql.BarcodeUpdate, new
            {
                Id = id,
                IdProducto = body.IdProducto,
                Codigo = body.Codigo,
                Factor = body.Factor,
                Descripcion = body.Descripcion,
                IdProveedor = body.IdProveedor
            }, transaction: tx);

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:int}", async ([FromServices] Db db, int id, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(Sql.BarcodeDelete, new { Id = id }, transaction: tx);

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        // ===== Precios =====
        group.MapGet("/prices/by-product/{productId:int}", async ([FromServices] Db db, int productId) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync<PrecioCodigoBarra>(Sql.PricesByProduct, new { productId });
            return Results.Ok(rows);
        });

        group.MapPost("/prices", async ([FromServices] Db db, [FromBody] PriceUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var id = await conn.ExecuteScalarAsync<long>(Sql.PriceInsert, new
            {
                IdProducto = body.IdProducto,
                Tipo = body.Tipo,
                Precio = body.Precio,
                CantidadMinima = body.CantidadMinima,
                FechaInicio = body.FechaInicio,
                FechaFin = body.FechaFin,
                Activo = body.Activo ? 1 : 0
            }, transaction: tx);

            await tx.CommitAsync();
            return Results.Created($"/barcodes/prices/{id}", new { id });
        });

        group.MapPut("/prices/{id:int}", async ([FromServices] Db db, int id, [FromBody] PriceUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(Sql.PriceUpdate, new
            {
                Id = id,
                Tipo = body.Tipo,
                Precio = body.Precio,
                CantidadMinima = body.CantidadMinima,
                FechaInicio = body.FechaInicio,
                FechaFin = body.FechaFin,
                Activo = body.Activo ? 1 : 0
            }, transaction: tx);

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/prices/{id:int}", async ([FromServices] Db db, int id, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(Sql.PriceDelete, new { Id = id }, transaction: tx);

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
