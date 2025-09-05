
namespace Ventapos.Api.Models;

public sealed record Sucursal(int Id, string Nombre, string? Direccion, bool Por_Defecto, bool Estado);
public sealed record SucursalUpsert(string Nombre, string? Direccion, bool PorDefecto, bool Estado);

public sealed record Almacen(int Id, int Id_Sucursal, string Nombre, string Tipo, string? Descripcion, bool Activo);
public sealed record AlmacenUpsert(int IdSucursal, string Nombre, string Tipo, string? Descripcion, bool Activo);

public sealed record AjusteStockRequest(int IdSucursal, int IdAlmacen, int IdProducto, decimal Cantidad, sbyte Signo, string? Observaciones);
public sealed record TransferRequest(int IdSucursal, int IdAlmacenOrigen, int IdAlmacenDestino, int IdProducto, decimal Cantidad, string? Observaciones);

public sealed record TipoMovimientoUpsert(string Nombre, sbyte Signo);
public sealed record TipoComprobanteUpsert(string Codigo, string Nombre);

public sealed record OutboxUpdateState(long Id, string Estado, string? UltimoError, bool IncrementarIntentos);

public sealed record PrecioRequest(int ProductId, decimal Cantidad);
public sealed record PrecioResponse(int Id, int Id_Producto, string Tipo, decimal Precio, decimal? CantidadMinima, DateOnly? FechaInicio, DateOnly? FechaFin);
