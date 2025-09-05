
namespace Ventapos.Api.Data;
public static class PricingSql
{
    public const string BestPrice = @"
SELECT *
FROM precios_codigos_barra
WHERE id_producto=@ProductId
  AND activo=1
  AND (fecha_inicio IS NULL OR fecha_inicio <= @Fecha)
  AND (fecha_fin IS NULL OR fecha_fin >= @Fecha)
  AND (cantidad_minima IS NULL OR @Cantidad >= cantidad_minima)
ORDER BY 
  CASE tipo WHEN 'OFERTA' THEN 1 WHEN 'MAYORISTA' THEN 2 ELSE 3 END,
  precio ASC
LIMIT 1";
}
