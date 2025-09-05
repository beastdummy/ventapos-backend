using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ventapos.Api.Data;

namespace Ventapos.Api.Features;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAudit(this IEndpointRouteBuilder app)
    {
        // Solo lectura; descomenta la línea con RequireRole si ya usas roles
        var group = app.MapGroup("/auditoria")
                       .WithTags("Auditoría")
                       .RequireAuthorization(p => p.RequireRole("admin"));
                       // .RequireAuthorization();

        // GET /auditoria/logs?tabla=productos&accion=ACTUALIZACION&idUsuario=9&idSolicitud=...&pk=id=123&desde=2025-08-01&hasta=2025-08-31&page=1&size=50
        group.MapGet("/logs", async (
            [FromServices] Db db,
            string? tabla,
            string? accion,        // INSERCION | ACTUALIZACION | ELIMINACION
            int? idUsuario,
            string? idSolicitud,   // GUID de request
            string? pk,            // ej: "id=123"
            DateOnly? desde,
            DateOnly? hasta,
            int page = 1,
            int size = 50
        ) =>
        {
            if (page < 1) page = 1;
            if (size < 1) size = 50;
            if (size > 200) size = 200;

            var offset = (page - 1) * size;

            using var conn = await db.OpenAsync();

            var sql = Sql.AuditListBase;
            var where = new List<string>();
            var p = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(tabla))
            {
                where.Add("ae.nombre_tabla = @tabla");
                p.Add("tabla", tabla);
            }
            if (!string.IsNullOrWhiteSpace(accion))
            {
                where.Add("ae.accion = @accion");
                p.Add("accion", accion);
            }
            if (idUsuario.HasValue)
            {
                where.Add("ae.id_usuario = @idUsuario");
                p.Add("idUsuario", idUsuario);
            }
            if (!string.IsNullOrWhiteSpace(idSolicitud))
            {
                where.Add("ae.id_solicitud = @idSolicitud");
                p.Add("idSolicitud", idSolicitud);
            }
            if (!string.IsNullOrWhiteSpace(pk))
            {
                where.Add("ae.pk = @pk");
                p.Add("pk", pk);
            }
            if (desde.HasValue)
            {
                where.Add("DATE(ae.fecha_utc) >= @desde");
                p.Add("desde", desde.Value);
            }
            if (hasta.HasValue)
            {
                where.Add("DATE(ae.fecha_utc) <= @hasta");
                p.Add("hasta", hasta.Value);
            }

            if (where.Count > 0)
                sql += " WHERE " + string.Join(" AND ", where);

            var sqlCount = "SELECT COUNT(1) FROM (" + sql + ") x";
            sql += " ORDER BY ae.fecha_utc DESC LIMIT @size OFFSET @offset";

            p.Add("size", size);
            p.Add("offset", offset);

            var total = await conn.ExecuteScalarAsync<long>(sqlCount, p);
            var rows = await conn.QueryAsync(sql, p);

            return Results.Ok(new
            {
                page,
                size,
                total,
                items = rows
            });
        });

        // GET /auditoria/{id}
        group.MapGet("/{id:long}", async ([FromServices] Db db, long id) =>
        {
            using var conn = await db.OpenAsync();

            var ev = await conn.QueryFirstOrDefaultAsync(Sql.AuditGetEvent, new { id });
            if (ev is null) return Results.NotFound();

            var cambios = await conn.QueryAsync(Sql.AuditGetChanges, new { idEvento = id });

            return Results.Ok(new
            {
                evento = ev,
                cambios
            });
        });

        return app;
    }
}
