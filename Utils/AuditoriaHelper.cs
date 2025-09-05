
using System.Data;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace Ventapos.Api.Utils;

public static class AuditoriaHelper
{
    public static async Task PrepararAsync(IDbConnection conn, HttpContext ctx, IDbTransaction? tx = null)
    {
        var uid = GetUserId(ctx);
        var reqId = Guid.NewGuid().ToString();

        // Settear variables de sesión para triggers
        await conn.ExecuteAsync("SET @app_user_id=@uid", new { uid }, tx);
        await conn.ExecuteAsync("SET @app_request_id=@rid", new { rid = reqId }, tx);
    }

    public static int? GetUserId(HttpContext ctx)
    {
        var c = ctx.User?.FindFirst(ClaimTypes.NameIdentifier) ?? ctx.User?.FindFirst("sub");
        return int.TryParse(c?.Value, out var id) ? id : null;
    }
}
