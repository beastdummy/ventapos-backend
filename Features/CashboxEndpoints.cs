using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils; // 👈 usa el helper

namespace Ventapos.Api.Features;

public static class CashboxEndpoints
{
    public static IEndpointRouteBuilder MapCashbox(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cash").WithTags("Caja").RequireAuthorization();

        group.MapGet("/current", async ([FromServices] Db db) =>
        {
            using var conn = await db.OpenAsync();
            var open = await conn.QueryFirstOrDefaultAsync<CajaApertura>(Sql.CashCurrent);
            return open is null ? Results.NoContent() : Results.Ok(open);
        });

        group.MapPost("/open", async ([FromServices] Db db, [FromBody] CashOpenRequest body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx); // 👈 una línea

            var open = await conn.QueryFirstOrDefaultAsync<CajaApertura>(Sql.CashCurrent, transaction: tx);
            if (open is not null) return Results.Conflict(new { message = "Ya hay una caja abierta" });

            // Si tu SQL usa el usuario, pásalo desde aquí si hace falta
            var uid = AuditoriaHelper.GetUserId(ctx) ?? 0;

            var id = await conn.ExecuteScalarAsync<long>(
                Sql.CashOpen,
                new { IdUsuario = uid, MontoInicial = body.MontoInicial },
                transaction: tx
            );

            await tx.CommitAsync();
            return Results.Created($"/cash/{id}", new { id });
        });

        group.MapPost("/close", async ([FromServices] Db db, [FromBody] CashCloseRequest body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var open = await conn.QueryFirstOrDefaultAsync<CajaApertura>(Sql.CashCurrent, transaction: tx);
            if (open is null) return Results.Conflict(new { message = "No hay caja abierta" });

            await conn.ExecuteAsync(
                Sql.CashClose,
                new { IdApertura = open.Id, body.TotalVentas, body.TotalEfectivo, body.Observaciones },
                transaction: tx
            );

            await tx.CommitAsync();
            return Results.NoContent();
        });

        group.MapPost("/movement", async ([FromServices] Db db, [FromBody] CashMovementRequest body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var open = await conn.QueryFirstOrDefaultAsync<CajaApertura>(Sql.CashCurrent, transaction: tx);
            if (open is null) return Results.Conflict(new { message = "No hay caja abierta" });

            var id = await conn.ExecuteScalarAsync<long>(
                Sql.CashMovesInsert,
                new { IdApertura = open.Id, body.TipoMovimiento, body.Descripcion, body.Monto },
                transaction: tx
            );

            await tx.CommitAsync();
            return Results.Created($"/cash/movement/{id}", new { id });
        });

        return app;
    }
}
