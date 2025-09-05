
namespace Ventapos.Api.Data;
public static class BranchSql
{
    public const string BranchAll = "SELECT * FROM sucursales ORDER BY nombre";
    public const string BranchGet = "SELECT * FROM sucursales WHERE id=@id";
    public const string BranchInsert = @"
INSERT INTO sucursales(nombre, direccion, por_defecto, estado) 
VALUES (@Nombre, @Direccion, @PorDefecto, @Estado);
SELECT LAST_INSERT_ID();";
    public const string BranchUpdate = @"
UPDATE sucursales SET nombre=@Nombre, direccion=@Direccion, por_defecto=@PorDefecto, estado=@Estado WHERE id=@Id";
    public const string BranchDelete = "DELETE FROM sucursales WHERE id=@Id";
    public const string BranchUnsetDefaultAll = "UPDATE sucursales SET por_defecto=0";
    public const string BranchSetDefault = "UPDATE sucursales SET por_defecto=1 WHERE id=@Id";
}
