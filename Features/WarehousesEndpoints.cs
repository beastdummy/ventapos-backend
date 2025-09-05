
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;

namespace Ventapos.Api.Features;

public static class WarehousesEndpoints
{
    public static IEndpointRouteBuilder MapWarehouses(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/warehouses").WithTags("Almacenes").RequireAuthorization();

        g.MapGet("/", async (int? branchId, Db db) => {
            using var conn = await db.OpenAsync();
            if (branchId is int b) return await conn.QueryAsync(WarehouseSql.WareByBranch, new { BranchId = b });
            return await conn.QueryAsync(WarehouseSql.WareAll);
        });

        g.MapGet("/{id:int}", async (int id, Db db) => {
            using var conn = await db.OpenAsync();
            var item = await conn.QueryFirstOrDefaultAsync(WarehouseSql.WareGet, new { id });
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        g.MapPost("/", async (AlmacenUpsert req, Db db, HttpContext ctx) => {
            using var conn = await db.OpenAsync();
            await AuditoriaHelper.PrepararAsync(conn, ctx);
            var id = await conn.ExecuteScalarAsync<int>(WarehouseSql.WareInsert, new { IdSucursal = req.IdSucursal, Nombre = req.Nombre, Tipo = req.Tipo, Descripcion = req.Descripcion, Activo = req.Activo });
            return Results.Created($"/warehouses/{id}", new { id });
        });

        g.MapPut("/{id:int}", async (int id, AlmacenUpsert req, Db db, HttpContext ctx) => {
            using var conn = await db.OpenAsync();
            await AuditoriaHelper.PrepararAsync(conn, ctx);
            var rows = await conn.ExecuteAsync(WarehouseSql.WareUpdate, new { Id = id, IdSucursal = req.IdSucursal, Nombre = req.Nombre, Tipo = req.Tipo, Descripcion = req.Descripcion, Activo = req.Activo });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        g.MapDelete("/{id:int}", async (int id, Db db, HttpContext ctx) => {
            using var conn = await db.OpenAsync();
            await AuditoriaHelper.PrepararAsync(conn, ctx);
            var rows = await conn.ExecuteAsync(WarehouseSql.WareDelete, new { Id = id });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        g.MapGet("/{id:int}/stock", async (int id, Db db) => {
            using var conn = await db.OpenAsync();
            var list = await conn.QueryAsync(WarehouseSql.StockByWarehouse, new { IdAlmacen = id });
            return Results.Ok(list);
        });

        g.MapGet("/product/{productId:int}/stock", async (int productId, Db db) => {
            using var conn = await db.OpenAsync();
            var list = await conn.QueryAsync(WarehouseSql.StockByProduct, new { IdProducto = productId });
            var agg = await conn.QueryFirstOrDefaultAsync(WarehouseSql.StockAggregateProduct, new { IdProducto = productId });
            return Results.Ok(new { detalle = list, agregado = agg });
        });

        return g;
    }
}
