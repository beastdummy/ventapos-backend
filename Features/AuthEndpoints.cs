using System.Security.Claims;
using BCrypt.Net;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Auth;
using Ventapos.Api.Data;
using Ventapos.Api.Models;
using Ventapos.Api.Utils;

namespace Ventapos.Api.Features;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        // REGISTRO (con auditoría y transacción)
        group.MapPost("/register", async ([FromServices] Db db, [FromBody] RegisterRequest req, HttpContext ctx) =>
        {
            using var conn = await db.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();

            await AuditoriaHelper.PrepararAsync(conn, ctx, tx);

            var exists = await conn.QueryFirstOrDefaultAsync<Usuario>(
                Sql.UserByEmail, new { email = req.Email }, transaction: tx
            );
            if (exists is not null)
            {
                await tx.RollbackAsync();
                return Results.Conflict(new { message = "Email ya registrado" });
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var id = await conn.ExecuteScalarAsync<long>(
                Sql.InsertUser,
                new
                {
                    Nombre = req.Nombre,
                    Rut = req.Rut,
                    Email = req.Email,
                    PasswordHash = hash,
                    Rol = string.IsNullOrWhiteSpace(req.Rol) ? "cajero" : req.Rol
                },
                transaction: tx
            );

            await tx.CommitAsync();
            return Results.Created($"/users/{id}", new { id });
        });

        // LOGIN
        group.MapPost("/login", async ([FromServices] Db db, JwtService jwt, [FromBody] LoginRequest req) =>
        {
            using var conn = await db.OpenAsync();
            var user = await conn.QueryFirstOrDefaultAsync<Usuario>(Sql.UserByEmail, new { email = req.Email });
            if (user is null || string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(req.Password, user.Password))
                return Results.Unauthorized();

            var token = jwt.CreateToken((long)user.Id, user.Nombre ?? "", user.Email ?? "", user.Rol ?? "cajero");
            return Results.Ok(new AuthResponse(user.Id, user.Nombre ?? "", user.Email ?? "", user.Rol ?? "cajero", token));
        });

        // QUIEN SOY
        group.MapGet("/me", [Authorize] (ClaimsPrincipal user) =>
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            return Results.Ok(new
            {
                id,
                name = user.FindFirst("name")?.Value,
                email = user.FindFirst(ClaimTypes.Email)?.Value,
                role = user.FindFirst(ClaimTypes.Role)?.Value
            });
        });

        return app;
    }
}
