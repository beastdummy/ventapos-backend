using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;

namespace Ventapos.Api.Features;

public static class MesasEndpoints
{
    public static IEndpointRouteBuilder MapMesas(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/mesas").WithTags("Mesas").RequireAuthorization();

        g.MapGet("/sectores", async ([FromServices] Db db) =>
        {
            using var c = await db.OpenAsync();
            var rows = await c.QueryAsync<Sector>(Sql.SectoresAll);
            return Results.Ok(rows);
        });

        g.MapGet("/estados", async ([FromServices] Db db) =>
        {
            using var c = await db.OpenAsync();
            var rows = await c.QueryAsync<EstadoMesa>(Sql.EstadoMesasAll);
            return Results.Ok(rows);
        });

        g.MapGet("/", async ([FromServices] Db db) =>
        {
            using var c = await db.OpenAsync();
            var rows = await c.QueryAsync<Mesa>(Sql.MesasAll);
            return Results.Ok(rows);
        });

        g.MapGet("/{id:int}", async ([FromServices] Db db, int id) =>
        {
            using var c = await db.OpenAsync();
            var m = await c.QueryFirstOrDefaultAsync<Mesa>(Sql.MesaGet, new { id });
            return m is null ? Results.NotFound() : Results.Ok(m);
        });

        g.MapPost("/", async ([FromServices] Db db, [FromBody] MesaCreateRequest body, HttpContext ctx) =>
        {
            using var c = await db.OpenAsync();
            using var tx = await c.BeginTransactionAsync();
            await AuditoriaHelper.PrepararAsync(c, ctx, tx);

            var id = await c.ExecuteScalarAsync<long>(
                Sql.MesaInsert,
                new { Numero = body.Numero, IdSector = body.IdSector, IdEstado = body.IdEstado },
                tx
            );

            await tx.CommitAsync();
            return Results.Created($"/mesas/{id}", new { id });
        });

        g.MapPut("/{id:int}", async ([FromServices] Db db, int id, [FromBody] MesaUpdateRequest body, HttpContext ctx) =>
        {
            using var c = await db.OpenAsync();
            using var tx = await c.BeginTransactionAsync();
            await AuditoriaHelper.PrepararAsync(c, ctx, tx);

            var n = await c.ExecuteAsync(
                Sql.MesaUpdate,
                new { Id = id, Numero = body.Numero, IdSector = body.IdSector, IdEstado = body.IdEstado },
                tx
            );

            await tx.CommitAsync();
            return n == 0 ? Results.NotFound() : Results.NoContent();
        });

        g.MapPatch("/{id:int}/estado", async ([FromServices] Db db, int id, [FromBody] MesaSetEstadoRequest body, HttpContext ctx) =>
        {
            using var c = await db.OpenAsync();
            using var tx = await c.BeginTransactionAsync();
            await AuditoriaHelper.PrepararAsync(c, ctx, tx);

            var n = await c.ExecuteAsync(Sql.MesaSetEstado, new { Id = id, IdEstado = body.IdEstado }, tx);
            await tx.CommitAsync();
            return n == 0 ? Results.NotFound() : Results.NoContent();
        });

        g.MapPatch("/{id:int}/mover", async ([FromServices] Db db, int id, [FromBody] MesaMoverRequest body, HttpContext ctx) =>
        {
            using var c = await db.OpenAsync();
            using var tx = await c.BeginTransactionAsync();
            await AuditoriaHelper.PrepararAsync(c, ctx, tx);

            var n = await c.ExecuteAsync(Sql.MesaMover, new { Id = id, IdSector = body.IdSector }, tx);
            await tx.CommitAsync();
            return n == 0 ? Results.NotFound() : Results.NoContent();
        });

        g.MapDelete("/{id:int}", async ([FromServices] Db db, int id, HttpContext ctx) =>
        {
            using var c = await db.OpenAsync();
            using var tx = await c.BeginTransactionAsync();
            await AuditoriaHelper.PrepararAsync(c, ctx, tx);

            var n = await c.ExecuteAsync(Sql.MesaDelete, new { Id = id }, tx);
            await tx.CommitAsync();
            return n == 0 ? Results.NotFound() : Results.NoContent();
        });

        return app;
    }
}
