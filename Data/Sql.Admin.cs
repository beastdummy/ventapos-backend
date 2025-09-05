
namespace Ventapos.Api.Data;
public static class AdminSql
{
    public const string TmovAll = "SELECT * FROM tipos_movimiento ORDER BY id";
    public const string TmovInsert = @"
INSERT INTO tipos_movimiento(nombre, signo) VALUES (@Nombre, @Signo);
SELECT LAST_INSERT_ID();";
    public const string TmovUpdate = "UPDATE tipos_movimiento SET nombre=@Nombre, signo=@Signo WHERE id=@Id";
    public const string TmovDelete = "DELETE FROM tipos_movimiento WHERE id=@Id";

    public const string TcompAll = "SELECT * FROM tipos_comprobante ORDER BY id";
    public const string TcompInsert = @"
INSERT INTO tipos_comprobante(codigo, nombre) VALUES (@Codigo, @Nombre);
SELECT LAST_INSERT_ID();";
    public const string TcompUpdate = "UPDATE tipos_comprobante SET codigo=@Codigo, nombre=@Nombre WHERE id=@Id";
    public const string TcompDelete = "DELETE FROM tipos_comprobante WHERE id=@Id";

    public const string UsersAll = "SELECT id, nombre, rut, email, rol FROM usuarios ORDER BY id DESC";
    public const string UserGet = "SELECT id, nombre, rut, email, rol FROM usuarios WHERE id=@id";
    public const string UserUpdate = "UPDATE usuarios SET nombre=@Nombre, rut=@Rut, email=@Email, rol=@Rol WHERE id=@Id";
    public const string UserDelete = "DELETE FROM usuarios WHERE id=@Id";

    public const string OutboxList = @"
SELECT * FROM audit_outbox
WHERE (@Estado IS NULL OR estado=@Estado)
ORDER BY creado_el DESC
LIMIT @Limit OFFSET @Offset";
    public const string OutboxSetState = @"
UPDATE audit_outbox
SET estado=@Estado, enviado_el = CASE WHEN @Estado='enviado' THEN NOW() ELSE enviado_el END,
    ultimo_error = @UltimoError, intentos = CASE WHEN @IncrementarIntentos=1 THEN intentos+1 ELSE intentos END
WHERE id=@Id";
}
