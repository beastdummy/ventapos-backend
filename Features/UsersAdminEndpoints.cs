
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Ventapos.Api.Data;
using Ventapos.Api.Models;

namespace Ventapos.Api.Features;

public static class UsersAdminEndpoints
{
    public static IEndpointRouteBuilder MapUsersAdmin(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/users").WithTags("Usuarios").RequireAuthorization();

        g.MapGet("/", async (Db db) => {
            using var conn = await db.OpenAsync();
            return await conn.QueryAsync(AdminSql.UsersAll);
        });

        g.MapGet("/{id:int}", async (int id, Db db) => {
            using var conn = await db.OpenAsync();
            var u = await conn.QueryFirstOrDefaultAsync(AdminSql.UserGet, new { id });
            return u is null ? Results.NotFound() : Results.Ok(u);
        });

        g.MapPut("/{id:int}", async (int id, Usuario req, Db db) => {
            using var conn = await db.OpenAsync();
            var rows = await conn.ExecuteAsync(AdminSql.UserUpdate, new { Id=id, Nombre=req.Nombre, Rut=req.Rut, Email=req.Email, Rol=req.Rol });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        g.MapDelete("/{id:int}", async (int id, Db db) => {
            using var conn = await db.OpenAsync();
            var rows = await conn.ExecuteAsync(AdminSql.UserDelete, new { Id=id });
            return rows > 0 ? Results.NoContent() : Results.NotFound();
        });

        return g;
    }
}
