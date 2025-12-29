CREATE TABLE IF NOT EXISTS manuales (
    id UUID PRIMARY KEY,
    titulo VARCHAR(255) NOT NULL,
    categoria VARCHAR(100) NOT NULL,
    sub_categoria VARCHAR(100),
    version VARCHAR(50),
    descripcion TEXT,
    ruta_local VARCHAR(500) NOT NULL,
    nombre_original VARCHAR(255) NOT NULL,
    usuario_id VARCHAR(100) NOT NULL,
    fecha_subida TIMESTAMP NOT NULL,
    peso_archivo BIGINT NOT NULL,
    estado INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Índices para optimizar búsquedas
CREATE INDEX IF NOT EXISTS idx_manuales_categoria 
    ON manuales(categoria);

CREATE INDEX IF NOT EXISTS idx_manuales_categoria_subcategoria 
    ON manuales(categoria, sub_categoria);

CREATE INDEX IF NOT EXISTS idx_manuales_estado 
    ON manuales(estado);

CREATE INDEX IF NOT EXISTS idx_manuales_fecha_subida 
    ON manuales(fecha_subida DESC);

CREATE INDEX IF NOT EXISTS idx_manuales_usuario_id 
    ON manuales(usuario_id);

-- Trigger para actualizar updated_at automáticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_manuales_updated_at 
    BEFORE UPDATE ON manuales
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Comentarios para documentación
COMMENT ON TABLE manuales IS 'Almacena la metadata de los manuales de la base de conocimiento';
COMMENT ON COLUMN manuales.id IS 'Identificador único que sincroniza con Qdrant y el sistema de archivos';
COMMENT ON COLUMN manuales.titulo IS 'Nombre del manual';
COMMENT ON COLUMN manuales.categoria IS 'Categoría principal (ej: IT, RRHH, Finanzas)';
COMMENT ON COLUMN manuales.sub_categoria IS 'Subcategoría (ej: Redes, Contratación)';
COMMENT ON COLUMN manuales.version IS 'Versión del manual (ej: v1.0, 2025-A)';
COMMENT ON COLUMN manuales.descripcion IS 'Descripción breve del contenido';
COMMENT ON COLUMN manuales.ruta_local IS 'Ruta completa del archivo en el disco';
COMMENT ON COLUMN manuales.nombre_original IS 'Nombre original del archivo subido';
COMMENT ON COLUMN manuales.usuario_id IS 'ID o email del usuario que subió el manual';
COMMENT ON COLUMN manuales.fecha_subida IS 'Fecha y hora de carga del manual';
COMMENT ON COLUMN manuales.peso_archivo IS 'Tamaño del archivo en bytes';
COMMENT ON COLUMN manuales.estado IS '1=Activo, 2=Obsoleto, 3=En Revisión';

-- Vista para estadísticas
CREATE OR REPLACE VIEW vw_estadisticas_manuales AS
SELECT 
    categoria,
    sub_categoria,
    estado,
    COUNT(*) as cantidad,
    SUM(peso_archivo) as peso_total_bytes,
    ROUND(AVG(peso_archivo) / 1024.0 / 1024.0, 2) as peso_promedio_mb,
    MIN(fecha_subida) as primer_manual,
    MAX(fecha_subida) as ultimo_manual
FROM manuales
GROUP BY categoria, sub_categoria, estado;

COMMENT ON VIEW vw_estadisticas_manuales IS 'Vista con estadísticas agregadas por categoría';

-- Función para limpiar manuales huérfanos (sin archivo físico)
CREATE OR REPLACE FUNCTION obtener_manuales_huerfanos()
RETURNS TABLE (
    manual_id UUID,
    titulo VARCHAR,
    ruta_local VARCHAR,
    fecha_subida TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        m.id,
        m.titulo,
        m.ruta_local,
        m.fecha_subida
    FROM manuales m
    WHERE m.estado = 1
    ORDER BY m.fecha_subida DESC;
END;
$$ LANGUAGE plpgsql;