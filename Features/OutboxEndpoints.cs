
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Ventapos.Api.Data;
using Ventapos.Api.Models;

namespace Ventapos.Api.Features;

public static class OutboxEndpoints
{
    public static IEndpointRouteBuilder MapOutbox(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/outbox").WithTags("Audit Outbox").RequireAuthorization();

        g.MapGet("/", async (string? estado, int page, int pageSize, Db db) => {
            using var conn = await db.OpenAsync();
            var limit = pageSize <= 0 ? 20 : pageSize;
            var offset = Math.Max(0, page - 1) * limit;
            var list = await conn.QueryAsync(AdminSql.OutboxList, new { Estado = estado, Limit = limit, Offset = offset });
            return Results.Ok(list);
        });

        g.MapPatch("/{id:long}", async (long id, OutboxUpdateState req, Db db) => {
            using var conn = await db.OpenAsync();
            var rows = await conn.ExecuteAsync(AdminSql.OutboxSetState, new {
                Id = id, Estado = req.Estado, UltimoError = req.UltimoError, IncrementarIntentos = req.IncrementarIntentos ? 1 : 0
            });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        return g;
    }
}
