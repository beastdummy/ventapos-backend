
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;

namespace Ventapos.Api.Features;

public static class BranchesEndpoints
{
    public static IEndpointRouteBuilder MapBranches(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/branches").WithTags("Sucursales").RequireAuthorization();
        
        g.MapGet("/", async (Db db) => {
            using var conn = await db.OpenAsync();
            return await conn.QueryAsync(BranchSql.BranchAll);
        });

        g.MapGet("/{id:int}", async (int id, Db db) => {
            using var conn = await db.OpenAsync();
            var item = await conn.QueryFirstOrDefaultAsync(BranchSql.BranchGet, new { id });
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        g.MapPost("/", async (SucursalUpsert req, Db db, HttpContext ctx) => {
            using var conn = await db.OpenAsync();
            await AuditoriaHelper.PrepararAsync(conn, ctx);
            if (req.PorDefecto) await conn.ExecuteAsync(BranchSql.BranchUnsetDefaultAll);
            var id = await conn.ExecuteScalarAsync<int>(BranchSql.BranchInsert, new { req.Nombre, req.Direccion, PorDefecto = req.PorDefecto, Estado = req.Estado });
            if (req.PorDefecto) await conn.ExecuteAsync(BranchSql.BranchSetDefault, new { Id = id });
            return Results.Created($"/branches/{id}", new { id });
        });

        g.MapPut("/{id:int}", async (int id, SucursalUpsert req, Db db, HttpContext ctx) => {
            using var conn = await db.OpenAsync();
            await AuditoriaHelper.PrepararAsync(conn, ctx);
            if (req.PorDefecto) await conn.ExecuteAsync(BranchSql.BranchUnsetDefaultAll);
            var rows = await conn.ExecuteAsync(BranchSql.BranchUpdate, new { Id = id, Nombre = req.Nombre, Direccion = req.Direccion, PorDefecto = req.PorDefecto, Estado = req.Estado });
            if (req.PorDefecto) await conn.ExecuteAsync(BranchSql.BranchSetDefault, new { Id = id });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        g.MapDelete("/{id:int}", async (int id, Db db, HttpContext ctx) => {
            using var conn = await db.OpenAsync();
            await AuditoriaHelper.PrepararAsync(conn, ctx);
            var rows = await conn.ExecuteAsync(BranchSql.BranchDelete, new { Id = id });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        return g;
    }
}
