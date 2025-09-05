
namespace Ventapos.Api.Data;

public static class Sql
{
    // ===== Auth =====
    public const string UserByEmail = "SELECT * FROM usuarios WHERE email=@email LIMIT 1";
    public const string InsertUser = @"
INSERT INTO usuarios(nombre,rut,email,password,rol)
VALUES (@Nombre,@Rut,@Email,@PasswordHash,@Rol);
SELECT LAST_INSERT_ID();";

    // ===== Catálogo: Categorías =====
    public const string CatAll = "SELECT * FROM categorias ORDER BY nombre";
    public const string CatInsert = @"
INSERT INTO categorias(nombre,activo) VALUES (@Nombre, @Activo);
SELECT LAST_INSERT_ID();";
    public const string CatUpdate = "UPDATE categorias SET nombre=@Nombre, activo=@Activo WHERE id=@Id";
    public const string CatDelete = "DELETE FROM categorias WHERE id=@Id";

    // ===== Catálogo: Productos =====
    public const string ProdAll = @"
SELECT p.*, c.nombre AS categoria_nombre
FROM productos p
LEFT JOIN categorias c ON p.id_categoria = c.id
ORDER BY p.id DESC";
    public const string ProdGet = "SELECT * FROM productos WHERE id=@id";
    public const string ProdInsert = @"
INSERT INTO productos(nombre, id_categoria, precio_compraactual, precio_comprapromedio, estado, stock_cache)
VALUES (@Nombre, @IdCategoria, @PrecioCompraActual, @PrecioCompraPromedio, @Estado, @StockCache);
SELECT LAST_INSERT_ID();";
    public const string ProdUpdate = @"
UPDATE productos
SET nombre=@Nombre, id_categoria=@IdCategoria, precio_compraactual=@PrecioCompraActual, 
    precio_comprapromedio=@PrecioCompraPromedio, estado=@Estado, stock_cache=@StockCache
WHERE id=@Id";
    public const string ProdDelete = "DELETE FROM productos WHERE id=@Id";

    // ===== Barcodes =====
    public const string BarcodeByProduct = "SELECT * FROM codigos_barra WHERE id_producto=@productId ORDER BY id DESC";
    public const string BarcodeGet = "SELECT * FROM codigos_barra WHERE id=@id";
    public const string BarcodeInsert = @"
INSERT INTO codigos_barra(id_producto,codigo,factor,descripcion,id_proveedor)
VALUES (@IdProducto,@Codigo,@Factor,@Descripcion,@IdProveedor);
SELECT LAST_INSERT_ID();";
    public const string BarcodeUpdate = @"
UPDATE codigos_barra
SET id_producto=@IdProducto, codigo=@Codigo, factor=@Factor, descripcion=@Descripcion, id_proveedor=@IdProveedor
WHERE id=@Id";
    public const string BarcodeDelete = "DELETE FROM codigos_barra WHERE id=@Id";

    // ===== Precios de producto =====
    public const string PricesByProduct = "SELECT * FROM precios_codigos_barra WHERE id_producto=@productId ORDER BY id DESC";
    public const string PriceInsert = @"
INSERT INTO precios_codigos_barra(id_producto,tipo,precio,cantidad_minima,fecha_inicio,fecha_fin,activo)
VALUES (@IdProducto,@Tipo,@Precio,@CantidadMinima,@FechaInicio,@FechaFin,@Activo);
SELECT LAST_INSERT_ID();";
    public const string PriceUpdate = @"
UPDATE precios_codigos_barra
SET tipo=@Tipo, precio=@Precio, cantidad_minima=@CantidadMinima, fecha_inicio=@FechaInicio, fecha_fin=@FechaFin, activo=@Activo
WHERE id=@Id";
    public const string PriceDelete = "DELETE FROM precios_codigos_barra WHERE id=@Id";

    // ===== Config =====
    public const string TaxesAll = "SELECT * FROM impuesto ORDER BY id DESC";
    public const string FormsAll = "SELECT * FROM formas_pago ORDER BY id DESC";
    public const string CompanyGet = "SELECT * FROM empresa ORDER BY id DESC LIMIT 1";
    public const string CompanyUpsert = @"
INSERT INTO empresa (id, rut, razonsocial, nombre, giro, direccion, email, telefono)
VALUES (1, @Rut, @RazonSocial, @Nombre, @Giro, @Direccion, @Email, @Telefono)
ON DUPLICATE KEY UPDATE
  rut=VALUES(rut), razonsocial=VALUES(razonsocial), nombre=VALUES(nombre),
  giro=VALUES(giro), direccion=VALUES(direccion), email=VALUES(email), telefono=VALUES(telefono);";

    // ===== Mesas =====
    public const string SectoresAll = "SELECT * FROM sectores ORDER BY nombre";
    public const string EstadoMesasAll = "SELECT * FROM estados_mesa ORDER BY id";
    public const string MesasAll = "SELECT * FROM mesas ORDER BY id";
    public const string MesaGet = "SELECT * FROM mesas WHERE id=@id";
    public const string MesaInsert = @"
INSERT INTO mesas(numero, id_sector, id_estado) VALUES (@Numero, @IdSector, @IdEstado);
SELECT LAST_INSERT_ID();";
    public const string MesaUpdate = @"
UPDATE mesas SET numero=@Numero, id_sector=@IdSector, id_estado=@IdEstado WHERE id=@Id";
    public const string MesaSetEstado = "UPDATE mesas SET id_estado=@IdEstado WHERE id=@Id";
    public const string MesaMover = "UPDATE mesas SET id_sector=@IdSector WHERE id=@Id";
    public const string MesaDelete = "DELETE FROM mesas WHERE id=@Id";

    // ===== People =====
    public const string ClientsAll = "SELECT * FROM clientes ORDER BY id DESC";
    public const string ClientInsert = @"
INSERT INTO clientes(nombre,rut,direccion,telefono,email)
VALUES (@Nombre,@Rut,@Direccion,@Telefono,@Email);
SELECT LAST_INSERT_ID();";
    public const string ClientUpdate = @"
UPDATE clientes SET nombre=@Nombre, rut=@Rut, direccion=@Direccion, telefono=@Telefono, email=@Email
WHERE id=@Id";
    public const string ClientDelete = "DELETE FROM clientes WHERE id=@Id";

    public const string SuppliersAll = "SELECT * FROM proveedores ORDER BY id DESC";
    public const string SupplierInsert = @"
INSERT INTO proveedores(nombre,rut,direccion,telefono,email)
VALUES (@Nombre,@Rut,@Direccion,@Telefono,@Email);
SELECT LAST_INSERT_ID();";
    public const string SupplierUpdate = @"
UPDATE proveedores SET nombre=@Nombre, rut=@Rut, direccion=@Direccion, telefono=@Telefono, email=@Email
WHERE id=@Id";
    public const string SupplierDelete = "DELETE FROM proveedores WHERE id=@Id";

    // ===== Cashbox =====
    public const string CashCurrent = "SELECT * FROM caja_apertura WHERE estado='abierta' ORDER BY id DESC LIMIT 1";
    public const string CashOpen = @"
INSERT INTO caja_apertura(id_usuario, fecha_apertura, monto_inicial, estado)
VALUES (@IdUsuario, NOW(), @MontoInicial, 'abierta');
SELECT LAST_INSERT_ID();";
    public const string CashClose = @"
INSERT INTO caja_cierre(id_apertura, fecha_cierre, total_ventas, total_efectivo, observaciones)
VALUES (@IdApertura, NOW(), @TotalVentas, @TotalEfectivo, @Observaciones);
UPDATE caja_apertura SET estado='cerrada' WHERE id=@IdApertura;";
    public const string CashMovesInsert = @"
INSERT INTO movimientos_caja(id_apertura, tipo_movimiento, descripcion, monto, fecha)
VALUES (@IdApertura, @TipoMovimiento, @Descripcion, @Monto, NOW());
SELECT LAST_INSERT_ID();";

    // ===== Sales =====
    public const string SaleHeadInsert = @"
INSERT INTO ventas_head(fecha, id_cliente, id_usuario, id_caja, subtotal, descuento_total, grantotal)
VALUES (NOW(), @IdCliente, @IdUsuario, @IdCaja, @Subtotal, @DescuentoTotal, @GranTotal);
SELECT LAST_INSERT_ID();";
    public const string SaleLineInsert = @"
INSERT INTO detalle_venta(id_venta, id_codigo_barra, cantidad, precio_unitario, descuento, total)
VALUES (@IdVenta, @IdCodigoBarra, @Cantidad, @PrecioUnitario, @Descuento, @Total);";

    // Movimientos
    public const string MoveInsert = @"
INSERT INTO movimientos(id_producto, id_tipo_movimiento, cantidad, tipo_origen, id_origen, id_usuario, fecha)
VALUES (@IdProducto, @IdTipoMovimiento, @Cantidad, @TipoOrigen, @IdOrigen, @IdUsuario, NOW());";

    // ===== Purchases =====
    public const string PurchaseHeadInsert = @"
INSERT INTO compras_head(fecha, id_proveedor, id_usuario, nro_doc, subtotal, descuento, grantotal)
VALUES (NOW(), @IdProveedor, @IdUsuario, @NroDoc, 0, 0, 0);
SELECT LAST_INSERT_ID();";

    public const string PurchaseDetailInsert = @"
INSERT INTO compras_detalle(id_compra, id_producto, id_codigo_barra, descripcion, cantidad, costo_unitario, descuento, total_linea)
VALUES (@IdCompra, @IdProducto, @IdCodigoBarra, @Descripcion, @Cantidad, @CostoUnitario, @Descuento, @TotalLinea);";

    public const string PurchaseRecalcTotals = @"
UPDATE compras_head ch
JOIN (
  SELECT id_compra, 
         SUM(cantidad * costo_unitario) AS subtotal,
         SUM(descuento) AS descuento,
         SUM(total_linea) AS grantotal
  FROM compras_detalle
  WHERE id_compra=@IdCompra
  GROUP BY id_compra
) x ON x.id_compra = ch.id
SET ch.subtotal = x.subtotal, ch.descuento = x.descuento, ch.grantotal = x.grantotal
WHERE ch.id=@IdCompra;";

    public const string PurchaseApplyStock = @"
-- asegurar tipo de movimiento COMPRA
INSERT INTO tipos_movimiento(nombre, signo)
SELECT 'COMPRA', 1
WHERE NOT EXISTS (SELECT 1 FROM tipos_movimiento WHERE nombre='COMPRA');

-- insertar movimientos por cada línea
INSERT INTO movimientos(id_producto, id_tipo_movimiento, cantidad, tipo_origen, id_origen, id_usuario, fecha)
SELECT 
  COALESCE(cd.id_producto, cb.id_producto) AS id_producto,
  (SELECT id FROM tipos_movimiento WHERE nombre='COMPRA' LIMIT 1) AS id_tipo_movimiento,
  (cd.cantidad * COALESCE(cb.factor,1)) AS cantidad,
  'compra' AS tipo_origen,
  cd.id_compra AS id_origen,
  ch.id_usuario AS id_usuario,
  NOW() AS fecha
FROM compras_detalle cd
LEFT JOIN codigos_barra cb ON cd.id_codigo_barra = cb.id
JOIN compras_head ch ON ch.id = cd.id_compra
WHERE cd.id_compra = @IdCompra;

-- actualizar stock_cache
UPDATE productos p
JOIN (
  SELECT COALESCE(cd.id_producto, cb.id_producto) AS id_producto,
         SUM(cd.cantidad * COALESCE(cb.factor,1)) AS qty
  FROM compras_detalle cd
  LEFT JOIN codigos_barra cb ON cd.id_codigo_barra = cb.id
  WHERE cd.id_compra = @IdCompra
  GROUP BY COALESCE(cd.id_producto, cb.id_producto)
) x ON x.id_producto = p.id
SET p.stock_cache = COALESCE(p.stock_cache,0) + x.qty;";

    public const string PurchaseGet = "SELECT * FROM compras_head WHERE id=@id";
    public const string PurchaseLinesByCompra = "SELECT * FROM compras_detalle WHERE id_compra=@id ORDER BY id";

    // ===== Audit =====
    public const string AuditListBase = @"
SELECT ae.id, ae.fecha_utc, ae.id_usuario, ae.accion, ae.nombre_tabla, ae.pk, ae.id_solicitud, ae.aplicacion, ae.ip_origen
FROM auditoria_evento ae";
    public const string AuditGetEvent = "SELECT * FROM auditoria_evento WHERE id=@id";
    public const string AuditGetChanges = "SELECT * FROM auditoria_cambio WHERE id_evento=@idEvento ORDER BY id";
}
