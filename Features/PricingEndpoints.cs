
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Ventapos.Api.Data;
using Ventapos.Api.Models;

namespace Ventapos.Api.Features;

public static class PricingEndpoints
{
    public static IEndpointRouteBuilder MapPricing(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/pricing").WithTags("Precios").RequireAuthorization();

        g.MapPost("/best", async (PrecioRequest req, Db db) => {
            using var conn = await db.OpenAsync();
            var p = await conn.QueryFirstOrDefaultAsync(PricingSql.BestPrice, new { ProductId = req.ProductId, Cantidad = req.Cantidad, Fecha = DateOnly.FromDateTime(DateTime.UtcNow) });
            return p is null ? Results.NotFound() : Results.Ok(p);
        });

        return g;
    }
}
