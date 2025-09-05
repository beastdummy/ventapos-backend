
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Ventapos.Api.Data;
using Ventapos.Api.Models;

namespace Ventapos.Api.Features;

public static class MovementTypesEndpoints
{
    public static IEndpointRouteBuilder MapMovementTypes(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/movement-types").WithTags("Tipos de Movimiento").RequireAuthorization();

        g.MapGet("/", async (Db db) => {
            using var conn = await db.OpenAsync();
            return await conn.QueryAsync(AdminSql.TmovAll);
        });

        g.MapPost("/", async (TipoMovimientoUpsert req, Db db) => {
            using var conn = await db.OpenAsync();
            var id = await conn.ExecuteScalarAsync<int>(AdminSql.TmovInsert, new { req.Nombre, req.Signo });
            return Results.Created($"/movement-types/{id}", new { id });
        });

        g.MapPut("/{id:int}", async (int id, TipoMovimientoUpsert req, Db db) => {
            using var conn = await db.OpenAsync();
            var rows = await conn.ExecuteAsync(AdminSql.TmovUpdate, new { Id=id, req.Nombre, req.Signo });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        g.MapDelete("/{id:int}", async (int id, Db db) => {
            using var conn = await db.OpenAsync();
            var rows = await conn.ExecuteAsync(AdminSql.TmovDelete, new { Id=id });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        return g;
    }
}
