using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;   // 👈 usamos el helper

namespace Ventapos.Api.Features;

public static class ConfigEndpoints
{
    public static IEndpointRouteBuilder MapConfig(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/config").WithTags("Configuración").RequireAuthorization();

        group.MapGet("/taxes", async ([FromServices] Db db) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync<Impuesto>(Sql.TaxesAll);
            return Results.Ok(rows);
        });

        group.MapGet("/payment-methods", async ([FromServices] Db db) =>
        {
            using var conn = await db.OpenAsync();
            var rows = await conn.QueryAsync<FormaPago>(Sql.FormsAll);
            return Results.Ok(rows);
        });

        group.MapGet("/company", async ([FromServices] Db db) =>
        {
            using var conn = await db.OpenAsync();
            var row = await conn.QueryFirstOrDefaultAsync<Empresa>(Sql.CompanyGet);
            return row is null ? Results.NoContent() : Results.Ok(row);
        });

        group.MapPost("/company", async ([FromServices] Db db, [FromBody] CompanyUpsert body, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx); // 👈 reemplazo

            await conn.ExecuteAsync(Sql.CompanyUpsert, new
            {
                body.Rut,
                body.RazonSocial,
                body.Nombre,
                body.Giro,
                body.Direccion,
                body.Email,
                body.Telefono
            }, transaction: tx);

            await tx.CommitAsync();
            return Results.NoContent();
        });

        return app;
    }
}
