
-- ======================================================================
-- THB POS - Esquema definitivo v2 (armonizado + mejoras)
-- Fecha de generación: 2025-09-05
-- Autor: ChatGPT (GPT-5 Thinking)
-- Cambios respecto a v1:
--  1) Unicidad en emails (usuarios, clientes, proveedores)
--  2) Índices por fecha en ventas, compras y movimientos de caja
--  3) Cantidades fraccionarias (DECIMAL(14,4)) en ventas/compras/movimientos/stock
--  4) CHECKs de negocio (signo en tipos_movimiento, precio>=0 y fechas válidas en precios)
--  5) Trigger de validación: ventas requieren caja en estado 'abierta'
--  6) Se mantienen: outbox + auditoría interna, triggers BI/BU, DECIMAL normalizado
-- ======================================================================

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES */;

-- Base de datos
CREATE DATABASE IF NOT EXISTS `ventapos`
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
USE `ventapos`;

-- ======================================================================
-- Tablas de seguridad y catálogos base
-- ======================================================================

DROP TABLE IF EXISTS usuario_permisos;
DROP TABLE IF EXISTS permisos;
DROP TABLE IF EXISTS usuarios;

CREATE TABLE usuarios (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  nombre       VARCHAR(100),
  rut          VARCHAR(20),
  email        VARCHAR(100),
  password     VARCHAR(255),
  rol          VARCHAR(50),
  UNIQUE KEY uq_usuarios_rut (rut),
  UNIQUE KEY uq_usuarios_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE permisos (
  id              INT AUTO_INCREMENT PRIMARY KEY,
  modulo          VARCHAR(50),
  nombre_permiso  VARCHAR(100),
  tipo            VARCHAR(50)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE usuario_permisos (
  id_usuario  INT NOT NULL,
  id_permiso  INT NOT NULL,
  PRIMARY KEY (id_usuario, id_permiso),
  CONSTRAINT fk_up_user FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_up_perm FOREIGN KEY (id_permiso) REFERENCES permisos(id) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Sucursales y almacenes
DROP TABLE IF EXISTS almacenes;
DROP TABLE IF EXISTS sucursales;

CREATE TABLE sucursales (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  nombre        VARCHAR(150) NOT NULL,
  direccion     VARCHAR(200),
  por_defecto   TINYINT(1) NOT NULL DEFAULT 0,
  estado        TINYINT(1) NOT NULL DEFAULT 1,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por    INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el  DATETIME NULL,
  eliminado_por INT NULL,
  UNIQUE KEY uq_suc_nombre (nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE almacenes (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  id_sucursal  INT NOT NULL,
  nombre       VARCHAR(120) NOT NULL,
  tipo         VARCHAR(50) NOT NULL DEFAULT 'general',
  descripcion  VARCHAR(200),
  activo       TINYINT(1) NOT NULL DEFAULT 1,
  creado_el    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_alm_suc_nombre (id_sucursal, nombre),
  KEY ix_alm_sucursal (id_sucursal),
  CONSTRAINT fk_alm_sucursal FOREIGN KEY (id_sucursal) REFERENCES sucursales(id) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Empresa
DROP TABLE IF EXISTS empresa;
CREATE TABLE empresa (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  rut           VARCHAR(45) NOT NULL,
  razonsocial   VARCHAR(120) NOT NULL,
  nombre        VARCHAR(120),
  giro          VARCHAR(120),
  direccion     VARCHAR(200),
  email         VARCHAR(100),
  telefono      VARCHAR(45),
  creado_por    INT NULL,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  eliminado_por INT NULL,
  eliminado_el  DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Catálogos POS
DROP TABLE IF EXISTS categorias;
CREATE TABLE categorias (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  nombre        VARCHAR(100) NOT NULL,
  activo        TINYINT DEFAULT 1,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por    INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el  DATETIME NULL,
  eliminado_por INT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS proveedores;
CREATE TABLE proveedores (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  nombre        VARCHAR(100),
  rut           VARCHAR(20),
  direccion     VARCHAR(200),
  telefono      VARCHAR(20),
  email         VARCHAR(100),
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por    INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el  DATETIME NULL,
  eliminado_por INT NULL,
  UNIQUE KEY uq_proveedores_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS clientes;
CREATE TABLE clientes (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  nombre        VARCHAR(100),
  rut           VARCHAR(20),
  direccion     VARCHAR(200),
  telefono      VARCHAR(20),
  email         VARCHAR(100),
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por    INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el  DATETIME NULL,
  eliminado_por INT NULL,
  UNIQUE KEY uq_clientes_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS impuesto;
CREATE TABLE impuesto (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  nombre        VARCHAR(45),
  valor         DECIMAL(10,4), -- tasa/porcentaje
  creado_por    INT NULL,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  eliminado_por INT NULL,
  eliminado_el  DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS sectores;
CREATE TABLE sectores (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  nombre        VARCHAR(50) NOT NULL,
  creado_por    INT NULL,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  eliminado_por INT NULL,
  eliminado_el  DATETIME NULL,
  UNIQUE KEY uq_sectores_nombre (nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS estados_mesa;
CREATE TABLE estados_mesa (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  nombre        VARCHAR(40) NOT NULL,
  creado_por    INT NULL,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  eliminado_por INT NULL,
  eliminado_el  DATETIME NULL,
  UNIQUE KEY uq_estados_mesa_nombre (nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS mesas;
CREATE TABLE mesas (
  id          INT AUTO_INCREMENT PRIMARY KEY,
  numero      INT NOT NULL,
  id_sector   INT NOT NULL,
  id_estado   INT NOT NULL DEFAULT 1,
  UNIQUE KEY uq_mesas_sector_numero (id_sector, numero),
  KEY ix_mesas_estado (id_estado),
  CONSTRAINT fk_mesas_sector FOREIGN KEY (id_sector) REFERENCES sectores(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_mesas_estado FOREIGN KEY (id_estado) REFERENCES estados_mesa(id) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS productos;
CREATE TABLE productos (
  id                     INT AUTO_INCREMENT PRIMARY KEY,
  nombre                 VARCHAR(100) NOT NULL,
  id_categoria           INT NULL,
  precio_compraactual    DECIMAL(14,4) DEFAULT NULL,
  precio_comprapromedio  DECIMAL(14,4) DEFAULT NULL,
  estado                 TINYINT(1) DEFAULT 1,
  stock_cache            DECIMAL(14,4) DEFAULT 0.0000,
  creado_el              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por             INT NULL,
  actualizado_el         DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por        INT NULL,
  eliminado_el           DATETIME NULL,
  eliminado_por          INT NULL,
  KEY ix_prod_categoria (id_categoria),
  CONSTRAINT fk_prod_categoria FOREIGN KEY (id_categoria) REFERENCES categorias(id) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS codigos_barra;
CREATE TABLE codigos_barra (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  id_producto   INT NOT NULL,
  codigo        VARCHAR(50) NOT NULL,
  factor        DECIMAL(14,4) DEFAULT 1.0000,
  descripcion   VARCHAR(50),
  id_proveedor  INT NULL,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por    INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el  DATETIME NULL,
  eliminado_por INT NULL,
  UNIQUE KEY uq_codigo_barra (codigo),
  KEY ix_cb_producto (id_producto),
  CONSTRAINT fk_cb_producto FOREIGN KEY (id_producto) REFERENCES productos(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_cb_proveedor FOREIGN KEY (id_proveedor) REFERENCES proveedores(id) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS precios_codigos_barra;
CREATE TABLE precios_codigos_barra (
  id              INT AUTO_INCREMENT PRIMARY KEY,
  id_producto     INT NOT NULL,
  tipo            ENUM('NORMAL','OFERTA','MAYORISTA') NOT NULL,
  precio          DECIMAL(14,4) NOT NULL,
  cantidad_minima DECIMAL(14,4) NULL,
  fecha_inicio    DATE NULL,
  fecha_fin       DATE NULL,
  activo          TINYINT(1) DEFAULT 1,
  creado_el       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por      INT NULL,
  actualizado_el  DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el    DATETIME NULL,
  eliminado_por   INT NULL,
  KEY ix_pcb_producto (id_producto),
  CONSTRAINT fk_pcb_producto FOREIGN KEY (id_producto) REFERENCES productos(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT chk_pcb_precio_pos CHECK (precio >= 0),
  CONSTRAINT chk_pcb_fechas CHECK (fecha_inicio IS NULL OR fecha_fin IS NULL OR fecha_inicio <= fecha_fin)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS tipos_movimiento;
CREATE TABLE tipos_movimiento (
  id      INT AUTO_INCREMENT PRIMARY KEY,
  nombre  VARCHAR(50) NOT NULL,
  signo   TINYINT NOT NULL,
  CONSTRAINT chk_tmov_signo CHECK (signo IN (-1,1)),
  UNIQUE KEY uq_tmov_nombre (nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS movimientos;
CREATE TABLE movimientos (
  id               INT AUTO_INCREMENT PRIMARY KEY,
  id_producto      INT NOT NULL,
  id_tipo_movimiento INT NOT NULL,
  cantidad         DECIMAL(14,4) NOT NULL,
  tipo_origen      VARCHAR(20),
  id_origen        INT,
  id_usuario       INT,
  fecha            DATETIME DEFAULT CURRENT_TIMESTAMP,
  creado_el        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por       INT NULL,
  KEY ix_mov_producto (id_producto),
  KEY ix_mov_tmov (id_tipo_movimiento),
  CONSTRAINT fk_mov_producto FOREIGN KEY (id_producto) REFERENCES productos(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_mov_tmov FOREIGN KEY (id_tipo_movimiento) REFERENCES tipos_movimiento(id) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS productos_almacen;
CREATE TABLE productos_almacen (
  id_almacen INT NOT NULL,
  id_producto INT NOT NULL,
  stock      DECIMAL(14,4) NOT NULL DEFAULT 0.0000,
  min_stock  DECIMAL(14,4) DEFAULT 0.0000,
  max_stock  DECIMAL(14,4) DEFAULT NULL,
  PRIMARY KEY (id_almacen, id_producto),
  KEY ix_pa_producto (id_producto),
  CONSTRAINT fk_pa_alm FOREIGN KEY (id_almacen) REFERENCES almacenes(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_pa_prod FOREIGN KEY (id_producto) REFERENCES productos(id) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS movimientos_almacen;
CREATE TABLE movimientos_almacen (
  id           BIGINT AUTO_INCREMENT PRIMARY KEY,
  fecha        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  id_sucursal  INT NOT NULL,
  id_almacen   INT NOT NULL,
  id_producto  INT NOT NULL,
  tipo         VARCHAR(30) NOT NULL,
  cantidad     DECIMAL(14,4) NOT NULL,
  signo        SMALLINT NOT NULL,
  tipo_origen  VARCHAR(30),
  id_origen    BIGINT,
  id_usuario   INT,
  observaciones VARCHAR(250),
  KEY ix_mv_fecha (fecha),
  KEY ix_mv_alm_fecha (id_almacen, fecha),
  KEY ix_mv_prod_fecha (id_producto, fecha),
  KEY ix_mv_suc (id_sucursal),
  CONSTRAINT fk_mv_alm FOREIGN KEY (id_almacen) REFERENCES almacenes(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_mv_prod FOREIGN KEY (id_producto) REFERENCES productos(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_mv_suc FOREIGN KEY (id_sucursal) REFERENCES sucursales(id) ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Caja
DROP TABLE IF EXISTS caja_cierre;
DROP TABLE IF EXISTS movimientos_caja;
DROP TABLE IF EXISTS caja_apertura;

CREATE TABLE caja_apertura (
  id             INT AUTO_INCREMENT PRIMARY KEY,
  id_usuario     INT NULL,
  fecha_apertura DATETIME NULL,
  monto_inicial  DECIMAL(14,4) NULL,
  estado         VARCHAR(20),
  creado_el      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por     INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el   DATETIME NULL,
  eliminado_por  INT NULL,
  KEY ix_caja_apertura_user (id_usuario),
  CONSTRAINT fk_caja_apertura_user FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE caja_cierre (
  id             INT AUTO_INCREMENT PRIMARY KEY,
  id_apertura    INT NULL,
  fecha_cierre   DATETIME NULL,
  total_ventas   DECIMAL(14,4) NULL,
  total_efectivo DECIMAL(14,4) NULL,
  observaciones  TEXT,
  creado_el      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por     INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el   DATETIME NULL,
  eliminado_por  INT NULL,
  KEY ix_caja_cierre_apertura (id_apertura),
  KEY ix_caja_cierre_fecha (fecha_cierre),
  CONSTRAINT fk_caja_cierre_apertura FOREIGN KEY (id_apertura) REFERENCES caja_apertura(id) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE movimientos_caja (
  id             INT AUTO_INCREMENT PRIMARY KEY,
  id_apertura    INT NULL,
  tipo_movimiento VARCHAR(50),
  descripcion    TEXT,
  monto          DECIMAL(14,4),
  fecha          DATETIME,
  creado_el      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por     INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el   DATETIME NULL,
  eliminado_por  INT NULL,
  KEY ix_mc_apertura (id_apertura),
  KEY ix_mc_fecha (fecha),
  CONSTRAINT fk_mc_apertura FOREIGN KEY (id_apertura) REFERENCES caja_apertura(id) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS formas_pago;
CREATE TABLE formas_pago (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  nombre        VARCHAR(50) NOT NULL,
  creado_por    INT NULL,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  eliminado_por INT NULL,
  eliminado_el  DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS tipos_comprobante;
CREATE TABLE tipos_comprobante (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  codigo        VARCHAR(10) NOT NULL,
  nombre        VARCHAR(30) NOT NULL,
  creado_el     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por    INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el  DATETIME NULL,
  eliminado_por INT NULL,
  UNIQUE KEY uq_tcomp_codigo (codigo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Compras
DROP TABLE IF EXISTS compras_detalle;
DROP TABLE IF EXISTS compras_head;

CREATE TABLE compras_head (
  id           BIGINT AUTO_INCREMENT PRIMARY KEY,
  fecha        DATETIME NULL,
  id_proveedor INT NULL,
  id_usuario   INT NULL,
  nro_doc      VARCHAR(45),
  subtotal     DECIMAL(14,4) NOT NULL DEFAULT 0.0000,
  descuento    DECIMAL(14,4) NOT NULL DEFAULT 0.0000,
  grantotal    DECIMAL(14,4) NOT NULL DEFAULT 0.0000,
  creado_el    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por   INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el DATETIME NULL,
  eliminado_por INT NULL,
  KEY ix_ch_proveedor (id_proveedor),
  KEY ix_ch_usuario (id_usuario),
  KEY ix_ch_fecha (fecha),
  CONSTRAINT fk_ch_proveedor FOREIGN KEY (id_proveedor) REFERENCES proveedores(id) ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT fk_ch_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE compras_detalle (
  id             BIGINT AUTO_INCREMENT PRIMARY KEY,
  id_compra      BIGINT NOT NULL,
  id_producto    INT NOT NULL,
  descripcion    VARCHAR(200),
  cantidad       DECIMAL(14,4) NOT NULL,
  costo_unitario DECIMAL(14,4) NOT NULL,
  descuento      DECIMAL(14,4) NOT NULL DEFAULT 0.0000,
  total_linea    DECIMAL(14,4) NOT NULL,
  creado_el      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por     INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el   DATETIME NULL,
  eliminado_por  INT NULL,
  id_codigo_barra INT NULL,
  KEY ix_cd_compra (id_compra),
  KEY ix_cd_producto (id_producto),
  KEY ix_cd_barcode (id_codigo_barra),
  CONSTRAINT fk_cd_barcode FOREIGN KEY (id_codigo_barra) REFERENCES codigos_barra(id) ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT fk_cd_compra FOREIGN KEY (id_compra) REFERENCES compras_head(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_cd_producto FOREIGN KEY (id_producto) REFERENCES productos(id) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Ventas
DROP TABLE IF EXISTS detalle_venta;
DROP TABLE IF EXISTS ventas_head;

CREATE TABLE ventas_head (
  id             INT AUTO_INCREMENT PRIMARY KEY,
  fecha          DATETIME DEFAULT CURRENT_TIMESTAMP,
  id_cliente     INT NULL,
  id_usuario     INT NULL,
  id_caja        INT NOT NULL,
  subtotal       DECIMAL(14,4),
  descuento_total DECIMAL(14,4),
  grantotal      DECIMAL(14,4),
  creado_el      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por     INT NULL,
  actualizado_el DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el   DATETIME NULL,
  eliminado_por  INT NULL,
  KEY ix_vh_cliente (id_cliente),
  KEY ix_vh_usuario (id_usuario),
  KEY ix_vh_caja (id_caja),
  KEY ix_vh_fecha (fecha),
  CONSTRAINT fk_vh_cliente FOREIGN KEY (id_cliente) REFERENCES clientes(id) ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT fk_vh_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id) ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT fk_vh_caja FOREIGN KEY (id_caja) REFERENCES caja_apertura(id) ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE detalle_venta (
  id              INT AUTO_INCREMENT PRIMARY KEY,
  id_venta        INT NOT NULL,
  id_codigo_barra INT NULL,
  cantidad        DECIMAL(14,4),
  precio_unitario DECIMAL(14,4),
  descuento       DECIMAL(14,4),
  total           DECIMAL(14,4),
  creado_el       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  creado_por      INT NULL,
  actualizado_el  DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
  actualizado_por INT NULL,
  eliminado_el    DATETIME NULL,
  eliminado_por   INT NULL,
  KEY ix_dv_venta (id_venta),
  KEY ix_dv_barcode (id_codigo_barra),
  CONSTRAINT fk_dv_venta FOREIGN KEY (id_venta) REFERENCES ventas_head(id) ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_dv_barcode FOREIGN KEY (id_codigo_barra) REFERENCES codigos_barra(id) ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Auditoría
DROP TABLE IF EXISTS auditoria_cambio;
DROP TABLE IF EXISTS auditoria_evento;
CREATE TABLE auditoria_evento (
  id            BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  fecha_utc     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  id_usuario    INT NULL,
  accion        ENUM('INSERCION','ACTUALIZACION','ELIMINACION') NOT NULL,
  nombre_tabla  VARCHAR(64) NOT NULL,
  pk            VARCHAR(128) NOT NULL,
  id_solicitud  CHAR(36) DEFAULT NULL,
  aplicacion    VARCHAR(64) DEFAULT NULL,
  ip_origen     VARCHAR(64) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE auditoria_cambio (
  id            BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  id_evento     BIGINT UNSIGNED NOT NULL,
  antes_json    LONGTEXT CHECK (json_valid(antes_json)),
  despues_json  LONGTEXT CHECK (json_valid(despues_json)),
  KEY fk_aud_cambio_evento (id_evento),
  CONSTRAINT fk_aud_cambio_evento FOREIGN KEY (id_evento) REFERENCES auditoria_evento(id) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

DROP TABLE IF EXISTS audit_outbox;
CREATE TABLE audit_outbox (
  id             BIGINT AUTO_INCREMENT PRIMARY KEY,
  tabla          VARCHAR(64) NOT NULL,
  accion         ENUM('I','U','D') NOT NULL,
  id_registro    BIGINT NULL,
  datos_antes    LONGTEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_bin CHECK (json_valid(datos_antes)),
  datos_despues  LONGTEXT CHARACTER SET utf8mb4 COLLATE utf8mb4_bin CHECK (json_valid(datos_despues)),
  usuario_id     INT NULL,
  creado_el      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  estado         ENUM('pendiente','enviado','error') NOT NULL DEFAULT 'pendiente',
  intentos       INT NOT NULL DEFAULT 0,
  enviado_el     DATETIME NULL,
  ultimo_error   TEXT,
  KEY ix_outbox_estado (estado),
  KEY ix_outbox_creado (creado_el),
  KEY ix_outbox_tabla_accion (tabla, accion)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ======================================================================
-- TRIGGERS estándar BI/BU (metadatos de creado/actualizado)
-- ======================================================================

DELIMITER ;
-- (se implementan manualmente por tabla clave más abajo)

-- clientes BI/BU
DROP TRIGGER IF EXISTS trg_clientes_bi;
DROP TRIGGER IF EXISTS trg_clientes_bu;
DELIMITER //
CREATE TRIGGER trg_clientes_bi
BEFORE INSERT ON clientes FOR EACH ROW
BEGIN
  SET NEW.creado_el  = COALESCE(NEW.creado_el, NOW());
  SET NEW.creado_por = COALESCE(NEW.creado_por, @app_user_id);
END//
CREATE TRIGGER trg_clientes_bu
BEFORE UPDATE ON clientes FOR EACH ROW
BEGIN
  SET NEW.actualizado_el  = NOW();
  SET NEW.actualizado_por = COALESCE(@app_user_id, NEW.actualizado_por);
END//
DELIMITER ;

-- empresa BI
DROP TRIGGER IF EXISTS trg_empresa_bi;
DELIMITER //
CREATE TRIGGER trg_empresa_bi
BEFORE INSERT ON empresa FOR EACH ROW
BEGIN
  SET NEW.creado_el  = COALESCE(NEW.creado_el, NOW());
  SET NEW.creado_por = COALESCE(NEW.creado_por, @app_user_id);
END//
DELIMITER ;

-- estados_mesa BI
DROP TRIGGER IF EXISTS trg_estados_mesa_bi;
DELIMITER //
CREATE TRIGGER trg_estados_mesa_bi
BEFORE INSERT ON estados_mesa FOR EACH ROW
BEGIN
  SET NEW.creado_el  = COALESCE(NEW.creado_el, NOW());
  SET NEW.creado_por = COALESCE(NEW.creado_por, @app_user_id);
END//
DELIMITER ;

-- formas_pago BI
DROP TRIGGER IF EXISTS trg_formas_pago_bi;
DELIMITER //
CREATE TRIGGER trg_formas_pago_bi
BEFORE INSERT ON formas_pago FOR EACH ROW
BEGIN
  SET NEW.creado_el  = COALESCE(NEW.creado_el, NOW());
  SET NEW.creado_por = COALESCE(NEW.creado_por, @app_user_id);
END//
DELIMITER ;

-- impuesto BI
DROP TRIGGER IF EXISTS trg_impuesto_bi;
DELIMITER //
CREATE TRIGGER trg_impuesto_bi
BEFORE INSERT ON impuesto FOR EACH ROW
BEGIN
  SET NEW.creado_el  = COALESCE(NEW.creado_el, NOW());
  SET NEW.creado_por = COALESCE(NEW.creado_por, @app_user_id);
END//
DELIMITER ;

-- ======================================================================
-- TRIGGER de validación de caja abierta
-- ======================================================================
DROP TRIGGER IF EXISTS trg_ventas_head_bi;
DELIMITER //
CREATE TRIGGER trg_ventas_head_bi
BEFORE INSERT ON ventas_head FOR EACH ROW
BEGIN
  DECLARE v_estado VARCHAR(20);
  SELECT estado INTO v_estado FROM caja_apertura WHERE id = NEW.id_caja;
  IF v_estado IS NULL OR v_estado <> 'abierta' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se puede crear venta: caja no abierta';
  END IF;
END//
DELIMITER ;

-- ======================================================================
-- TRIGGERS de auditoría OUTBOX (I/U/D) para catálogos y operaciones
-- ======================================================================

-- almacenes OUTBOX
DROP TRIGGER IF EXISTS trg_almacenes_ai;
DROP TRIGGER IF EXISTS trg_almacenes_au;
DROP TRIGGER IF EXISTS trg_almacenes_ad;
DELIMITER //
CREATE TRIGGER trg_almacenes_ai AFTER INSERT ON almacenes FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('almacenes','I', NEW.id, NULL,
          JSON_OBJECT('id', NEW.id, 'id_sucursal', NEW.id_sucursal, 'nombre', NEW.nombre, 'tipo', NEW.tipo, 'descripcion', NEW.descripcion, 'activo', NEW.activo),
          @app_user_id);
END//
CREATE TRIGGER trg_almacenes_au AFTER UPDATE ON almacenes FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('almacenes','U', NEW.id,
          JSON_OBJECT('id', OLD.id, 'id_sucursal', OLD.id_sucursal, 'nombre', OLD.nombre, 'tipo', OLD.tipo, 'descripcion', OLD.descripcion, 'activo', OLD.activo),
          JSON_OBJECT('id', NEW.id, 'id_sucursal', NEW.id_sucursal, 'nombre', NEW.nombre, 'tipo', NEW.tipo, 'descripcion', NEW.descripcion, 'activo', NEW.activo),
          @app_user_id);
END//
CREATE TRIGGER trg_almacenes_ad AFTER DELETE ON almacenes FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('almacenes','D', OLD.id,
          JSON_OBJECT('id', OLD.id, 'id_sucursal', OLD.id_sucursal, 'nombre', OLD.nombre, 'tipo', OLD.tipo, 'descripcion', OLD.descripcion, 'activo', OLD.activo),
          NULL, @app_user_id);
END//
DELIMITER ;

-- categorias OUTBOX
DROP TRIGGER IF EXISTS trg_categorias_ai;
DROP TRIGGER IF EXISTS trg_categorias_au;
DROP TRIGGER IF EXISTS trg_categorias_ad;
DELIMITER //
CREATE TRIGGER trg_categorias_ai AFTER INSERT ON categorias FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('categorias','I', NEW.id, NULL, JSON_OBJECT('id', NEW.id,'nombre', NEW.nombre, 'activo', NEW.activo), @app_user_id);
END//
CREATE TRIGGER trg_categorias_au AFTER UPDATE ON categorias FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('categorias','U', NEW.id,
          JSON_OBJECT('id', OLD.id,'nombre', OLD.nombre, 'activo', OLD.activo),
          JSON_OBJECT('id', NEW.id,'nombre', NEW.nombre, 'activo', NEW.activo), @app_user_id);
END//
CREATE TRIGGER trg_categorias_ad AFTER DELETE ON categorias FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('categorias','D', OLD.id, JSON_OBJECT('id', OLD.id,'nombre', OLD.nombre, 'activo', OLD.activo), NULL, @app_user_id);
END//
DELIMITER ;

-- clientes OUTBOX
DROP TRIGGER IF EXISTS trg_clientes_ai;
DROP TRIGGER IF EXISTS trg_clientes_au;
DROP TRIGGER IF EXISTS trg_clientes_ad;
DELIMITER //
CREATE TRIGGER trg_clientes_ai AFTER INSERT ON clientes FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('clientes','I', NEW.id, NULL,
          JSON_OBJECT('id', NEW.id,'nombre', NEW.nombre,'rut', NEW.rut,'direccion', NEW.direccion,'telefono', NEW.telefono,'email', NEW.email),
          @app_user_id);
END//
CREATE TRIGGER trg_clientes_au AFTER UPDATE ON clientes FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('clientes','U', NEW.id,
          JSON_OBJECT('id', OLD.id,'nombre', OLD.nombre,'rut', OLD.rut,'direccion', OLD.direccion,'telefono', OLD.telefono,'email', OLD.email),
          JSON_OBJECT('id', NEW.id,'nombre', NEW.nombre,'rut', NEW.rut,'direccion', NEW.direccion,'telefono', NEW.telefono,'email', NEW.email),
          @app_user_id);
END//
CREATE TRIGGER trg_clientes_ad AFTER DELETE ON clientes FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox (tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('clientes','D', OLD.id,
          JSON_OBJECT('id', OLD.id,'nombre', OLD.nombre,'rut', OLD.rut,'direccion', OLD.direccion,'telefono', OLD.telefono,'email', OLD.email),
          NULL, @app_user_id);
END//
DELIMITER ;

-- estados_mesa OUTBOX
DROP TRIGGER IF EXISTS trg_estados_mesa_ai;
DROP TRIGGER IF EXISTS trg_estados_mesa_au;
DROP TRIGGER IF EXISTS trg_estados_mesa_ad;
DELIMITER //
CREATE TRIGGER trg_estados_mesa_ai AFTER INSERT ON estados_mesa FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('estados_mesa','I', NEW.id, NULL, JSON_OBJECT('id', NEW.id,'nombre', NEW.nombre), @app_user_id);
END//
CREATE TRIGGER trg_estados_mesa_au AFTER UPDATE ON estados_mesa FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('estados_mesa','U', NEW.id, JSON_OBJECT('id', OLD.id,'nombre', OLD.nombre), JSON_OBJECT('id', NEW.id,'nombre', NEW.nombre), @app_user_id);
END//
CREATE TRIGGER trg_estados_mesa_ad AFTER DELETE ON estados_mesa FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('estados_mesa','D', OLD.id, JSON_OBJECT('id', OLD.id,'nombre', OLD.nombre), NULL, @app_user_id);
END//
DELIMITER ;

-- formas_pago OUTBOX
DROP TRIGGER IF EXISTS trg_formas_pago_ai;
DROP TRIGGER IF EXISTS trg_formas_pago_au;
DROP TRIGGER IF EXISTS trg_formas_pago_ad;
DELIMITER //
CREATE TRIGGER trg_formas_pago_ai AFTER INSERT ON formas_pago FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('formas_pago','I', NEW.id, NULL, JSON_OBJECT('id', NEW.id,'nombre', NEW.nombre), @app_user_id);
END//
CREATE TRIGGER trg_formas_pago_au AFTER UPDATE ON formas_pago FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('formas_pago','U', NEW.id, JSON_OBJECT('id', OLD.id,'nombre', OLD.nombre), JSON_OBJECT('id', NEW.id,'nombre', NEW.nombre), @app_user_id);
END//
CREATE TRIGGER trg_formas_pago_ad AFTER DELETE ON formas_pago FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('formas_pago','D', OLD.id, JSON_OBJECT('id', OLD.id,'nombre', OLD.nombre), NULL, @app_user_id);
END//
DELIMITER ;

-- impuesto OUTBOX
DROP TRIGGER IF EXISTS trg_impuesto_ai;
DROP TRIGGER IF EXISTS trg_impuesto_au;
DROP TRIGGER IF EXISTS trg_impuesto_ad;
DELIMITER //
CREATE TRIGGER trg_impuesto_ai AFTER INSERT ON impuesto FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('impuesto','I', NEW.id, NULL, JSON_OBJECT('id', NEW.id, 'nombre', NEW.nombre, 'valor', NEW.valor), @app_user_id);
END//
CREATE TRIGGER trg_impuesto_au AFTER UPDATE ON impuesto FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('impuesto','U', NEW.id,
          JSON_OBJECT('id', OLD.id, 'nombre', OLD.nombre, 'valor', OLD.valor),
          JSON_OBJECT('id', NEW.id, 'nombre', NEW.nombre, 'valor', NEW.valor), @app_user_id);
END//
CREATE TRIGGER trg_impuesto_ad AFTER DELETE ON impuesto FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('impuesto','D', OLD.id, JSON_OBJECT('id', OLD.id, 'nombre', OLD.nombre, 'valor', OLD.valor), NULL, @app_user_id);
END//
DELIMITER ;

-- mesas OUTBOX
DROP TRIGGER IF EXISTS trg_mesas_ai;
DROP TRIGGER IF EXISTS trg_mesas_au;
DROP TRIGGER IF EXISTS trg_mesas_ad;
DELIMITER //
CREATE TRIGGER trg_mesas_ai AFTER INSERT ON mesas FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('mesas','I', NEW.id, NULL, JSON_OBJECT('id', NEW.id,'id_sector', NEW.id_sector,'numero', NEW.numero,'id_estado', NEW.id_estado), @app_user_id);
END//
CREATE TRIGGER trg_mesas_au AFTER UPDATE ON mesas FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('mesas','U', NEW.id,
          JSON_OBJECT('id', OLD.id,'id_sector', OLD.id_sector,'numero', OLD.numero,'id_estado', OLD.id_estado),
          JSON_OBJECT('id', NEW.id,'id_sector', NEW.id_sector,'numero', NEW.numero,'id_estado', NEW.id_estado), @app_user_id);
END//
CREATE TRIGGER trg_mesas_ad AFTER DELETE ON mesas FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('mesas','D', OLD.id,
          JSON_OBJECT('id', OLD.id,'id_sector', OLD.id_sector,'numero', OLD.numero,'id_estado', OLD.id_estado),
          NULL, @app_user_id);
END//
DELIMITER ;

-- movimientos_almacen OUTBOX
DROP TRIGGER IF EXISTS trg_movalmacen_ai;
DROP TRIGGER IF EXISTS trg_movalmacen_au;
DROP TRIGGER IF EXISTS trg_movalmacen_ad;
DELIMITER //
CREATE TRIGGER trg_movalmacen_ai AFTER INSERT ON movimientos_almacen FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('movimientos_almacen','I', NEW.id, NULL,
          JSON_OBJECT('id', NEW.id,'fecha', NEW.fecha,'id_sucursal', NEW.id_sucursal,'id_almacen', NEW.id_almacen,
                      'id_producto', NEW.id_producto,'tipo', NEW.tipo,'signo', NEW.signo,'cantidad', NEW.cantidad,
                      'tipo_origen', NEW.tipo_origen,'id_origen', NEW.id_origen,'id_usuario', NEW.id_usuario,'observaciones', NEW.observaciones),
          COALESCE(@app_user_id, NEW.id_usuario));
END//
CREATE TRIGGER trg_movalmacen_au AFTER UPDATE ON movimientos_almacen FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('movimientos_almacen','U', NEW.id,
          JSON_OBJECT('id', OLD.id,'fecha', OLD.fecha,'id_sucursal', OLD.id_sucursal,'id_almacen', OLD.id_almacen,
                      'id_producto', OLD.id_producto,'tipo', OLD.tipo,'signo', OLD.signo,'cantidad', OLD.cantidad,
                      'tipo_origen', OLD.tipo_origen,'id_origen', OLD.id_origen,'id_usuario', OLD.id_usuario,'observaciones', OLD.observaciones),
          JSON_OBJECT('id', NEW.id,'fecha', NEW.fecha,'id_sucursal', NEW.id_sucursal,'id_almacen', NEW.id_almacen,
                      'id_producto', NEW.id_producto,'tipo', NEW.tipo,'signo', NEW.signo,'cantidad', NEW.cantidad,
                      'tipo_origen', NEW.tipo_origen,'id_origen', NEW.id_origen,'id_usuario', NEW.id_usuario,'observaciones', NEW.observaciones),
          COALESCE(@app_user_id, NEW.id_usuario));
END//
CREATE TRIGGER trg_movalmacen_ad AFTER DELETE ON movimientos_almacen FOR EACH ROW
BEGIN
  INSERT INTO audit_outbox(tabla, accion, id_registro, datos_antes, datos_despues, usuario_id)
  VALUES ('movimientos_almacen','D', OLD.id,
          JSON_OBJECT('id', OLD.id,'fecha', OLD.fecha,'id_sucursal', OLD.id_sucursal,'id_almacen', OLD.id_almacen,
                      'id_producto', OLD.id_producto,'tipo', OLD.tipo,'signo', OLD.signo,'cantidad', OLD.cantidad,
                      'tipo_origen', OLD.tipo_origen,'id_origen', OLD.id_origen,'id_usuario', OLD.id_usuario,'observaciones', OLD.observaciones),
          NULL, COALESCE(@app_user_id, OLD.id_usuario));
END//
DELIMITER ;

-- ======================================================================
-- TRIGGERS de auditoría interna (auditoria_evento / auditoria_cambio)
-- ======================================================================

-- caja_apertura
DROP TRIGGER IF EXISTS trg_caja_apertura_ai;
DROP TRIGGER IF EXISTS trg_caja_apertura_bu;
DROP TRIGGER IF EXISTS trg_caja_apertura_bd;
DELIMITER //
CREATE TRIGGER trg_caja_apertura_ai AFTER INSERT ON caja_apertura FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'INSERCION', 'caja_apertura', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(), NULL,
          JSON_OBJECT('id', NEW.id, 'id_usuario', NEW.id_usuario, 'fecha_apertura', NEW.fecha_apertura,
                      'monto_inicial', NEW.monto_inicial, 'estado', NEW.estado));
END//
CREATE TRIGGER trg_caja_apertura_bu BEFORE UPDATE ON caja_apertura FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ACTUALIZACION', 'caja_apertura', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_usuario', OLD.id_usuario, 'fecha_apertura', OLD.fecha_apertura,
                      'monto_inicial', OLD.monto_inicial, 'estado', OLD.estado),
          JSON_OBJECT('id', NEW.id, 'id_usuario', NEW.id_usuario, 'fecha_apertura', NEW.fecha_apertura,
                      'monto_inicial', NEW.monto_inicial, 'estado', NEW.estado));
END//
CREATE TRIGGER trg_caja_apertura_bd BEFORE DELETE ON caja_apertura FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ELIMINACION', 'caja_apertura', CONCAT('id=', OLD.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_usuario', OLD.id_usuario, 'fecha_apertura', OLD.fecha_apertura,
                      'monto_inicial', OLD.monto_inicial, 'estado', OLD.estado),
          NULL);
END//
DELIMITER ;

-- caja_cierre
DROP TRIGGER IF EXISTS trg_caja_cierre_ai;
DROP TRIGGER IF EXISTS trg_caja_cierre_bu;
DROP TRIGGER IF EXISTS trg_caja_cierre_bd;
DELIMITER //
CREATE TRIGGER trg_caja_cierre_ai AFTER INSERT ON caja_cierre FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'INSERCION', 'caja_cierre', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(), NULL,
          JSON_OBJECT('id', NEW.id, 'id_apertura', NEW.id_apertura, 'fecha_cierre', NEW.fecha_cierre,
                      'total_ventas', NEW.total_ventas, 'total_efectivo', NEW.total_efectivo, 'observaciones', NEW.observaciones));
END//
CREATE TRIGGER trg_caja_cierre_bu BEFORE UPDATE ON caja_cierre FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ACTUALIZACION', 'caja_cierre', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_apertura', OLD.id_apertura, 'fecha_cierre', OLD.fecha_cierre,
                      'total_ventas', OLD.total_ventas, 'total_efectivo', OLD.total_efectivo, 'observaciones', OLD.observaciones),
          JSON_OBJECT('id', NEW.id, 'id_apertura', NEW.id_apertura, 'fecha_cierre', NEW.fecha_cierre,
                      'total_ventas', NEW.total_ventas, 'total_efectivo', NEW.total_efectivo, 'observaciones', NEW.observaciones));
END//
CREATE TRIGGER trg_caja_cierre_bd BEFORE DELETE ON caja_cierre FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ELIMINACION', 'caja_cierre', CONCAT('id=', OLD.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_apertura', OLD.id_apertura, 'fecha_cierre', OLD.fecha_cierre,
                      'total_ventas', OLD.total_ventas, 'total_efectivo', OLD.total_efectivo, 'observaciones', OLD.observaciones),
          NULL);
END//
DELIMITER ;

-- codigos_barra
DROP TRIGGER IF EXISTS trg_codigos_barra_ai;
DROP TRIGGER IF EXISTS trg_codigos_barra_bu;
DROP TRIGGER IF EXISTS trg_codigos_barra_bd;
DELIMITER //
CREATE TRIGGER trg_codigos_barra_ai AFTER INSERT ON codigos_barra FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'INSERCION', 'codigos_barra', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(), NULL,
          JSON_OBJECT('id', NEW.id, 'id_producto', NEW.id_producto, 'codigo', NEW.codigo, 'factor', NEW.factor));
END//
CREATE TRIGGER trg_codigos_barra_bu BEFORE UPDATE ON codigos_barra FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ACTUALIZACION', 'codigos_barra', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_producto', OLD.id_producto, 'codigo', OLD.codigo, 'factor', OLD.factor),
          JSON_OBJECT('id', NEW.id, 'id_producto', NEW.id_producto, 'codigo', NEW.codigo, 'factor', NEW.factor));
END//
CREATE TRIGGER trg_codigos_barra_bd BEFORE DELETE ON codigos_barra FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ELIMINACION', 'codigos_barra', CONCAT('id=', OLD.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_producto', OLD.id_producto, 'codigo', OLD.codigo, 'factor', OLD.factor),
          NULL);
END//
DELIMITER ;

-- ventas_head / detalle_venta
DROP TRIGGER IF EXISTS trg_ventas_head_ai;
DROP TRIGGER IF EXISTS trg_ventas_head_bu;
DROP TRIGGER IF EXISTS trg_ventas_head_bd;
DROP TRIGGER IF EXISTS trg_detalle_venta_ai;
DROP TRIGGER IF EXISTS trg_detalle_venta_bu;
DROP TRIGGER IF EXISTS trg_detalle_venta_bd;
DELIMITER //
CREATE TRIGGER trg_ventas_head_ai AFTER INSERT ON ventas_head FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'INSERCION', 'ventas_head', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(), NULL,
          JSON_OBJECT('id', NEW.id, 'id_cliente', NEW.id_cliente, 'id_usuario', NEW.id_usuario, 'id_caja', NEW.id_caja,
                      'subtotal', NEW.subtotal, 'descuento_total', NEW.descuento_total, 'grantotal', NEW.grantotal));
END//
CREATE TRIGGER trg_ventas_head_bu BEFORE UPDATE ON ventas_head FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ACTUALIZACION', 'ventas_head', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_cliente', OLD.id_cliente, 'id_usuario', OLD.id_usuario, 'id_caja', OLD.id_caja,
                      'subtotal', OLD.subtotal, 'descuento_total', OLD.descuento_total, 'grantotal', OLD.grantotal),
          JSON_OBJECT('id', NEW.id, 'id_cliente', NEW.id_cliente, 'id_usuario', NEW.id_usuario, 'id_caja', NEW.id_caja,
                      'subtotal', NEW.subtotal, 'descuento_total', NEW.descuento_total, 'grantotal', NEW.grantotal));
END//
CREATE TRIGGER trg_ventas_head_bd BEFORE DELETE ON ventas_head FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ELIMINACION', 'ventas_head', CONCAT('id=', OLD.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_cliente', OLD.id_cliente, 'id_usuario', OLD.id_usuario, 'id_caja', OLD.id_caja,
                      'subtotal', OLD.subtotal, 'descuento_total', OLD.descuento_total, 'grantotal', OLD.grantotal),
          NULL);
END//

CREATE TRIGGER trg_detalle_venta_ai AFTER INSERT ON detalle_venta FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'INSERCION', 'detalle_venta', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(), NULL,
          JSON_OBJECT('id', NEW.id, 'id_venta', NEW.id_venta, 'id_codigo_barra', NEW.id_codigo_barra,
                      'cantidad', NEW.cantidad, 'precio_unitario', NEW.precio_unitario, 'descuento', NEW.descuento, 'total', NEW.total));
END//
CREATE TRIGGER trg_detalle_venta_bu BEFORE UPDATE ON detalle_venta FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ACTUALIZACION', 'detalle_venta', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_venta', OLD.id_venta, 'id_codigo_barra', OLD.id_codigo_barra,
                      'cantidad', OLD.cantidad, 'precio_unitario', OLD.precio_unitario, 'descuento', OLD.descuento, 'total', OLD.total),
          JSON_OBJECT('id', NEW.id, 'id_venta', NEW.id_venta, 'id_codigo_barra', NEW.id_codigo_barra,
                      'cantidad', NEW.cantidad, 'precio_unitario', NEW.precio_unitario, 'descuento', NEW.descuento, 'total', NEW.total));
END//
CREATE TRIGGER trg_detalle_venta_bd BEFORE DELETE ON detalle_venta FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ELIMINACION', 'detalle_venta', CONCAT('id=', OLD.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_venta', OLD.id_venta, 'id_codigo_barra', OLD.id_codigo_barra,
                      'cantidad', OLD.cantidad, 'precio_unitario', OLD.precio_unitario, 'descuento', OLD.descuento, 'total', OLD.total),
          NULL);
END//
DELIMITER ;

-- compras_head / compras_detalle
DROP TRIGGER IF EXISTS trg_compras_head_ai;
DROP TRIGGER IF EXISTS trg_compras_head_bu;
DROP TRIGGER IF EXISTS trg_compras_head_bd;
DROP TRIGGER IF EXISTS trg_compras_detalle_ai;
DROP TRIGGER IF EXISTS trg_compras_detalle_bu;
DROP TRIGGER IF EXISTS trg_compras_detalle_bd;
DELIMITER //
CREATE TRIGGER trg_compras_head_ai AFTER INSERT ON compras_head FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'INSERCION', 'compras_head', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(), NULL,
          JSON_OBJECT('id', NEW.id, 'fecha', NEW.fecha, 'id_proveedor', NEW.id_proveedor, 'id_usuario', NEW.id_usuario,
                      'nro_doc', NEW.nro_doc, 'subtotal', NEW.subtotal, 'descuento', NEW.descuento, 'grantotal', NEW.grantotal));
END//
CREATE TRIGGER trg_compras_head_bu BEFORE UPDATE ON compras_head FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ACTUALIZACION', 'compras_head', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'fecha', OLD.fecha, 'id_proveedor', OLD.id_proveedor, 'id_usuario', OLD.id_usuario,
                      'nro_doc', OLD.nro_doc, 'subtotal', OLD.subtotal, 'descuento', OLD.descuento, 'grantotal', OLD.grantotal),
          JSON_OBJECT('id', NEW.id, 'fecha', NEW.fecha, 'id_proveedor', NEW.id_proveedor, 'id_usuario', NEW.id_usuario,
                      'nro_doc', NEW.nro_doc, 'subtotal', NEW.subtotal, 'descuento', NEW.descuento, 'grantotal', NEW.grantotal));
END//
CREATE TRIGGER trg_compras_head_bd BEFORE DELETE ON compras_head FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ELIMINACION', 'compras_head', CONCAT('id=', OLD.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'fecha', OLD.fecha, 'id_proveedor', OLD.id_proveedor, 'id_usuario', OLD.id_usuario,
                      'nro_doc', OLD.nro_doc, 'subtotal', OLD.subtotal, 'descuento', OLD.descuento, 'grantotal', OLD.grantotal),
          NULL);
END//

CREATE TRIGGER trg_compras_detalle_ai AFTER INSERT ON compras_detalle FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'INSERCION', 'compras_detalle', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(), NULL,
          JSON_OBJECT('id', NEW.id, 'id_compra', NEW.id_compra, 'id_producto', NEW.id_producto, 'descripcion', NEW.descripcion,
                      'cantidad', NEW.cantidad, 'costo_unitario', NEW.costo_unitario, 'descuento', NEW.descuento, 'total_linea', NEW.total_linea, 'id_codigo_barra', NEW.id_codigo_barra));
END//
CREATE TRIGGER trg_compras_detalle_bu BEFORE UPDATE ON compras_detalle FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ACTUALIZACION', 'compras_detalle', CONCAT('id=', NEW.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_compra', OLD.id_compra, 'id_producto', OLD.id_producto, 'descripcion', OLD.descripcion,
                      'cantidad', OLD.cantidad, 'costo_unitario', OLD.costo_unitario, 'descuento', OLD.descuento, 'total_linea', OLD.total_linea, 'id_codigo_barra', OLD.id_codigo_barra),
          JSON_OBJECT('id', NEW.id, 'id_compra', NEW.id_compra, 'id_producto', NEW.id_producto, 'descripcion', NEW.descripcion,
                      'cantidad', NEW.cantidad, 'costo_unitario', NEW.costo_unitario, 'descuento', NEW.descuento, 'total_linea', NEW.total_linea, 'id_codigo_barra', NEW.id_codigo_barra));
END//
CREATE TRIGGER trg_compras_detalle_bd BEFORE DELETE ON compras_detalle FOR EACH ROW
BEGIN
  INSERT INTO auditoria_evento(id_usuario, accion, nombre_tabla, pk, id_solicitud, aplicacion)
  VALUES (@app_user_id, 'ELIMINACION', 'compras_detalle', CONCAT('id=', OLD.id), @app_request_id, 'ventapos-api');
  INSERT INTO auditoria_cambio(id_evento, antes_json, despues_json)
  VALUES (LAST_INSERT_ID(),
          JSON_OBJECT('id', OLD.id, 'id_compra', OLD.id_compra, 'id_producto', OLD.id_producto, 'descripcion', OLD.descripcion,
                      'cantidad', OLD.cantidad, 'costo_unitario', OLD.costo_unitario, 'descuento', OLD.descuento, 'total_linea', OLD.total_linea, 'id_codigo_barra', OLD.id_codigo_barra),
          NULL);
END//
DELIMITER ;

-- ======================================================================
-- Restaurar modos
-- ======================================================================
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
