
namespace Ventapos.Api.Data;
public static class WarehouseSql
{
    public const string WareAll = "SELECT * FROM almacenes ORDER BY id";
    public const string WareByBranch = "SELECT * FROM almacenes WHERE id_sucursal=@BranchId ORDER BY id";
    public const string WareGet = "SELECT * FROM almacenes WHERE id=@id";
    public const string WareInsert = @"
INSERT INTO almacenes(id_sucursal, nombre, tipo, descripcion, activo)
VALUES (@IdSucursal, @Nombre, @Tipo, @Descripcion, @Activo);
SELECT LAST_INSERT_ID();";
    public const string WareUpdate = @"
UPDATE almacenes SET id_sucursal=@IdSucursal, nombre=@Nombre, tipo=@Tipo, descripcion=@Descripcion, activo=@Activo WHERE id=@Id";
    public const string WareDelete = "DELETE FROM almacenes WHERE id=@Id";

    public const string StockByWarehouse = @"
SELECT pa.id_almacen, a.nombre AS almacen, pa.id_producto, p.nombre AS producto, pa.stock, pa.min_stock, pa.max_stock
FROM productos_almacen pa
JOIN almacenes a ON a.id = pa.id_almacen
JOIN productos p ON p.id = pa.id_producto
WHERE pa.id_almacen=@IdAlmacen
ORDER BY p.nombre";
    public const string StockByProduct = @"
SELECT pa.id_almacen, a.nombre AS almacen, pa.id_producto, p.nombre AS producto, pa.stock, pa.min_stock, pa.max_stock
FROM productos_almacen pa
JOIN almacenes a ON a.id = pa.id_almacen
JOIN productos p ON p.id = pa.id_producto
WHERE pa.id_producto=@IdProducto
ORDER BY a.nombre";
    public const string StockAggregateProduct = @"
SELECT pa.id_producto, SUM(pa.stock) AS stock_total
FROM productos_almacen pa
WHERE pa.id_producto=@IdProducto
GROUP BY pa.id_producto";
    public const string UpsertProductoAlmacen = @"
INSERT INTO productos_almacen(id_almacen, id_producto, stock, min_stock, max_stock)
VALUES (@IdAlmacen, @IdProducto, @Stock, @MinStock, @MaxStock)
ON DUPLICATE KEY UPDATE stock=VALUES(stock), min_stock=VALUES(min_stock), max_stock=VALUES(max_stock);";
}
