using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;   // 👈 usamos el helper

namespace Ventapos.Api.Features;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalog(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/catalog").WithTags("Catálogo").RequireAuthorization();

        // Categorías
        group.MapGet("/categories", async ([FromServices] Db db) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync<Categoria>(Sql.CatAll);
            return Results.Ok(rows);
        });

        group.MapPost("/categories", async ([FromServices] Db db, [FromBody] CategoriaUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var id = await conn.ExecuteScalarAsync<long>(
                Sql.CatInsert,
                new { Nombre = body.Nombre, Activo = body.Activo ? 1 : 0 },
                transaction: tx
            );

            await tx.CommitAsync();
            return Results.Created($"/catalog/categories/{id}", new { id });
        });

        group.MapPut("/categories/{id:int}", async ([FromServices] Db db, int id, [FromBody] CategoriaUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(
                Sql.CatUpdate,
                new { Id = id, Nombre = body.Nombre, Activo = body.Activo ? 1 : 0 },
                transaction: tx
            );

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/categories/{id:int}", async ([FromServices] Db db, int id, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(Sql.CatDelete, new { Id = id }, transaction: tx);

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        // Productos
        group.MapGet("/products", async ([FromServices] Db db) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync(Sql.ProdAll);
            return Results.Ok(rows);
        });

        group.MapGet("/products/{id:int}", async ([FromServices] Db db, int id) =>
        {
            using var conn = await db.OpenAsync();
            var row = await conn.QueryFirstOrDefaultAsync(Sql.ProdGet, new { id });
            return row is not null ? Results.Ok(row) : Results.NotFound();
        });

        group.MapPost("/products", async ([FromServices] Db db, [FromBody] ProductoUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var id = await conn.ExecuteScalarAsync<long>(
                Sql.ProdInsert,
                new
                {
                    Nombre = body.Nombre,
                    IdCategoria = body.IdCategoria,
                    PrecioCompraActual = body.PrecioCompraActual,
                    PrecioCompraPromedio = body.PrecioCompraPromedio,
                    Estado = body.Estado ? 1 : 0,
                    StockCache = body.StockCache
                },
                transaction: tx
            );

            await tx.CommitAsync();
            return Results.Created($"/catalog/products/{id}", new { id });
        });

        group.MapPut("/products/{id:int}", async ([FromServices] Db db, int id, [FromBody] ProductoUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(
                Sql.ProdUpdate,
                new
                {
                    Id = id,
                    Nombre = body.Nombre,
                    IdCategoria = body.IdCategoria,
                    PrecioCompraActual = body.PrecioCompraActual,
                    PrecioCompraPromedio = body.PrecioCompraPromedio,
                    Estado = body.Estado ? 1 : 0,
                    StockCache = body.StockCache
                },
                transaction: tx
            );

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/products/{id:int}", async ([FromServices] Db db, int id, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(Sql.ProdDelete, new { Id = id }, transaction: tx);

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
