
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Ventapos.Api.Data;
using Ventapos.Api.Models;

namespace Ventapos.Api.Features;

public static class ComprobantesEndpoints
{
    public static IEndpointRouteBuilder MapComprobantes(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/comprobantes").WithTags("Tipos de Comprobante").RequireAuthorization();

        g.MapGet("/", async (Db db) => {
            using var conn = await db.OpenAsync();
            return await conn.QueryAsync(AdminSql.TcompAll);
        });

        g.MapPost("/", async (TipoComprobanteUpsert req, Db db) => {
            using var conn = await db.OpenAsync();
            var id = await conn.ExecuteScalarAsync<int>(AdminSql.TcompInsert, new { req.Codigo, req.Nombre });
            return Results.Created($"/comprobantes/{id}", new { id });
        });

        g.MapPut("/{id:int}", async (int id, TipoComprobanteUpsert req, Db db) => {
            using var conn = await db.OpenAsync();
            var rows = await conn.ExecuteAsync(AdminSql.TcompUpdate, new { Id = id, req.Codigo, req.Nombre });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        g.MapDelete("/{id:int}", async (int id, Db db) => {
            using var conn = await db.OpenAsync();
            var rows = await conn.ExecuteAsync(AdminSql.TcompDelete, new { Id = id });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        return g;
    }
}
