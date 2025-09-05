
# Ventapos.Api (Minimal API .NET 8)

Backend listo para correr con MySQL/MariaDB y Dapper, alineado al esquema `ventapos_definitivo_v2.sql`.

## Requisitos
- .NET 8 SDK
- MySQL/MariaDB
- Crear BD importando: `ventapos_definitivo_v2.sql`

## Configuración
Edita `appsettings.json` con tu cadena de conexión y secretos JWT.

## Ejecutar
```bash
dotnet restore
dotnet run
```
Swagger: http://localhost:5000/swagger

## Endpoints
- `/auth/*`
- `/catalog/*` (categorías, productos)
- `/barcodes/*` (códigos de barra y precios)
- `/config/*` (impuestos, formas de pago, empresa)
- `/mesas/*` (sectores, estados, mesas)
- `/people/*` (clientes, proveedores)
- `/cash/*` (caja)
- `/compras/*` (compras)
- `/sales/*` (ventas)
- `/auditoria/*` (eventos de auditoría)

## Notas
- Auditoría de triggers se alimenta con variables `@app_user_id` y `@app_request_id`; se seteán automáticamente en cada endpoint mediante `AuditoriaHelper.PrepararAsync(...)`.
- Para **ventas**, el endpoint calcula totales y registra movimientos de inventario (tipo `VENTA`) por cada línea.
- Para **compras**, se recalculan totales y se aplican entradas a inventario (tipo `COMPRA`) con actualización de `stock_cache`.


POST /auth/login

{ "email": "admin@ventapos.local", "password": "Admin#2025!" }


Copia token → botón Authorize → pega solo el token → Authorize.