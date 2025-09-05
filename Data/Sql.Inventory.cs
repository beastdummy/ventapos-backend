
namespace Ventapos.Api.Data;
public static class InventorySql
{
    public const string InsertMovAlmacen = @"
INSERT INTO movimientos_almacen(fecha, id_sucursal, id_almacen, id_producto, tipo, cantidad, signo, tipo_origen, id_origen, id_usuario, observaciones)
VALUES (NOW(), @IdSucursal, @IdAlmacen, @IdProducto, @Tipo, @Cantidad, @Signo, @TipoOrigen, @IdOrigen, @IdUsuario, @Observaciones);
SELECT LAST_INSERT_ID();";

    public const string AjusteStock = @"
INSERT INTO productos_almacen(id_almacen, id_producto, stock, min_stock, max_stock)
VALUES (@IdAlmacen, @IdProducto, 0, 0, NULL)
ON DUPLICATE KEY UPDATE stock=stock;
UPDATE productos_almacen SET stock = stock + (@Cantidad * @Signo) WHERE id_almacen=@IdAlmacen AND id_producto=@IdProducto;";

    public const string TransferStock = @"
UPDATE productos_almacen SET stock = stock - @Cantidad WHERE id_almacen=@IdAlmacenOrigen AND id_producto=@IdProducto;
INSERT INTO productos_almacen(id_almacen, id_producto, stock, min_stock, max_stock)
VALUES (@IdAlmacenDestino, @IdProducto, 0, 0, NULL)
ON DUPLICATE KEY UPDATE stock=stock;
UPDATE productos_almacen SET stock = stock + @Cantidad WHERE id_almacen=@IdAlmacenDestino AND id_producto=@IdProducto;";

    public const string MovsByProduct = @"
SELECT * FROM movimientos_almacen WHERE id_producto=@IdProducto ORDER BY fecha DESC LIMIT 200";
}
