using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;   // 👈 usamos el helper

namespace Ventapos.Api.Features;

public static class PeopleEndpoints
{
    public static IEndpointRouteBuilder MapPeople(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/people").WithTags("Personas").RequireAuthorization();

        // ===== Clientes =====
        group.MapGet("/clients", async ([FromServices] Db db) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync<Cliente>(Sql.ClientsAll);
            return Results.Ok(rows);
        });

        group.MapPost("/clients", async ([FromServices] Db db, [FromBody] ClientUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx); // 👈 una línea

            var id = await conn.ExecuteScalarAsync<long>(
                Sql.ClientInsert,
                new { body.Nombre, body.Rut, body.Direccion, body.Telefono, body.Email },
                transaction: tx
            );

            await tx.CommitAsync();
            return Results.Created($"/people/clients/{id}", new { id });
        });

        group.MapPut("/clients/{id:int}", async ([FromServices] Db db, int id, [FromBody] ClientUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(
                Sql.ClientUpdate,
                new { Id = id, body.Nombre, body.Rut, body.Direccion, body.Telefono, body.Email },
                transaction: tx
            );

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/clients/{id:int}", async ([FromServices] Db db, int id, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(Sql.ClientDelete, new { Id = id }, transaction: tx);

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        // ===== Proveedores =====
        group.MapGet("/suppliers", async ([FromServices] Db db) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync<Proveedor>(Sql.SuppliersAll);
            return Results.Ok(rows);
        });

        group.MapPost("/suppliers", async ([FromServices] Db db, [FromBody] SupplierUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var id = await conn.ExecuteScalarAsync<long>(
                Sql.SupplierInsert,
                new { body.Nombre, body.Rut, body.Direccion, body.Telefono, body.Email },
                transaction: tx
            );

            await tx.CommitAsync();
            return Results.Created($"/people/suppliers/{id}", new { id });
        });

        group.MapPut("/suppliers/{id:int}", async ([FromServices] Db db, int id, [FromBody] SupplierUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(
                Sql.SupplierUpdate,
                new { Id = id, body.Nombre, body.Rut, body.Direccion, body.Telefono, body.Email },
                transaction: tx
            );

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/suppliers/{id:int}", async ([FromServices] Db db, int id, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var n = await conn.ExecuteAsync(Sql.SupplierDelete, new { Id = id }, transaction: tx);

            await tx.CommitAsync();
            return n > 0 ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
