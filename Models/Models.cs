
namespace Ventapos.Api.Models;

// ===== Auth =====
public sealed record RegisterRequest(string Nombre, string? Rut, string Email, string Password, string? Rol);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(int Id, string Nombre, string Email, string Rol, string Token);

public sealed record Usuario(int Id, string? Nombre, string? Rut, string? Email, string? Password, string? Rol);

// ===== Catálogo =====
public sealed record Categoria(int Id, string Nombre, int Activo);
public sealed record CategoriaUpsert(string Nombre, bool Activo);

public sealed record Producto(int Id, string Nombre, int? IdCategoria, decimal? PrecioCompraActual, decimal? PrecioCompraPromedio, int? Estado, decimal? StockCache);
public sealed record ProductoUpsert(string Nombre, int? IdCategoria, decimal? PrecioCompraActual, decimal? PrecioCompraPromedio, bool Estado, decimal? StockCache);

// ===== Códigos de barra =====
public sealed record CodigoBarra(int Id, int Id_Producto, string Codigo, decimal Factor, string? Descripcion, int? Id_Proveedor);
public sealed record PrecioCodigoBarra(int Id, int Id_Producto, string Tipo, decimal Precio, decimal? CantidadMinima, DateOnly? FechaInicio, DateOnly? FechaFin, int Activo);
public sealed record BarcodeUpsert(int IdProducto, string Codigo, decimal Factor, string? Descripcion, int? IdProveedor);
public sealed record PriceUpsert(int IdProducto, string Tipo, decimal Precio, decimal? CantidadMinima, DateOnly? FechaInicio, DateOnly? FechaFin, bool Activo);

// ===== Config =====
public sealed record Impuesto(int Id, string? Nombre, decimal Valor);
public sealed record FormaPago(int Id, string Nombre);
public sealed record Empresa(int Id, string Rut, string RazonSocial, string? Nombre, string? Giro, string? Direccion, string? Email, string? Telefono);
public sealed record CompanyUpsert(string Rut, string RazonSocial, string? Nombre, string? Giro, string? Direccion, string? Email, string? Telefono);

// ===== Mesas =====
public sealed record Sector(int Id, string Nombre);
public sealed record EstadoMesa(int Id, string Nombre);
public sealed record Mesa(int Id, int Numero, int Id_Sector, int Id_Estado);
public sealed record MesaCreateRequest(int Numero, int IdSector, int IdEstado);
public sealed record MesaUpdateRequest(int Numero, int IdSector, int IdEstado);
public sealed record MesaSetEstadoRequest(int IdEstado);
public sealed record MesaMoverRequest(int IdSector);

// ===== Personas =====
public sealed record Cliente(int Id, string? Nombre, string? Rut, string? Direccion, string? Telefono, string? Email);
public sealed record Proveedor(int Id, string? Nombre, string? Rut, string? Direccion, string? Telefono, string? Email);
public sealed record ClientUpsert(string? Nombre, string? Rut, string? Direccion, string? Telefono, string? Email);
public sealed record SupplierUpsert(string? Nombre, string? Rut, string? Direccion, string? Telefono, string? Email);

// ===== Caja =====
public sealed record CajaApertura(int Id, int? Id_Usuario, DateTime? Fecha_Apertura, decimal? Monto_Inicial, string? Estado);
public sealed record CashOpenRequest(decimal MontoInicial);
public sealed record CashCloseRequest(decimal TotalVentas, decimal TotalEfectivo, string? Observaciones);
public sealed record CashMovementRequest(string TipoMovimiento, string? Descripcion, decimal Monto);

// ===== Movimientos =====
public sealed record TipoMovimiento(int Id, string Nombre, sbyte Signo);

// ===== Ventas =====
public sealed record VentaHead(int Id, DateTime? Fecha, int? Id_Cliente, int? Id_Usuario, int Id_Caja, decimal? Subtotal, decimal? Descuento_Total, decimal? GranTotal);
public sealed record DetalleVenta(int Id, int Id_Venta, int? Id_Codigo_Barra, decimal Cantidad, decimal Precio_Unitario, decimal Descuento, decimal Total);

public sealed record SaleCreateRequest(int? IdCliente, List<SaleLineDto> Lineas);
public sealed record SaleLineDto(int IdCodigoBarra, decimal Cantidad, decimal PrecioUnitario, decimal Descuento);
