TRUNCATE TABLE "categorias" CASCADE;

INSERT INTO "categorias" ("id", "nombre", "descripcion", "categoria_padre_id", "color", "icono", "orden", "es_activa", "fecha_creacion")
VALUES 
(gen_random_uuid(), 'aplicaciones', 'manuales de sistemas y apps core', NULL, '#3b82f6', 'layout-template', 1, true, NOW()),
(gen_random_uuid(), 'procedimientos', 'guías de pasos y normativas psa', NULL, '#8b5cf6', 'clipboard-list', 2, true, NOW()),
(gen_random_uuid(), 'hardware', 'configuración de periféricos e impresoras', NULL, '#f59e0b', 'cpu', 3, true, NOW());

INSERT INTO "categorias" ("id", "nombre", "descripcion", "categoria_padre_id", "color", "icono", "orden", "es_activa", "fecha_creacion")
SELECT gen_random_uuid(), 'sap', 'manuales de sap r3 y s4/hana', id, '#60a5fa', 'database', 1, true, NOW() 
FROM "categorias" WHERE "nombre" = 'aplicaciones';

INSERT INTO "categorias" ("id", "nombre", "descripcion", "categoria_padre_id", "color", "icono", "orden", "es_activa", "fecha_creacion")
SELECT gen_random_uuid(), 'lotus notes', 'configuración de correo e id', id, '#93c5fd', 'mail', 2, true, NOW() 
FROM "categorias" WHERE "nombre" = 'aplicaciones';

INSERT INTO "categorias" ("id", "nombre", "descripcion", "categoria_padre_id", "color", "icono", "orden", "es_activa", "fecha_creacion")
SELECT gen_random_uuid(), 'notas psa', 'procedimientos específicos de psa', id, '#a78bfa', 'file-text', 1, true, NOW() 
FROM "categorias" WHERE "nombre" = 'procedimientos';

INSERT INTO "categorias" ("id", "nombre", "descripcion", "categoria_padre_id", "color", "icono", "orden", "es_activa", "fecha_creacion")
SELECT gen_random_uuid(), 'fujitsu', 'impresoras térmicas f9870 y otros modelos', id, '#fbbf24', 'printer', 1, true, NOW() 
FROM "categorias" WHERE "nombre" = 'hardware';

INSERT INTO "usuarios" (
    "id", 
    "nombre_completo", 
    "email", 
    "departamento", 
    "password_hash", 
    "rol", 
    "es_activo", 
    "fecha_registro",
    "ultimo_acceso"
) VALUES (
    '00000000-0000-0000-0000-000000000000', 
    'pablo britez (sistema)', 
    'pablo@inuzaru.com', 
    'it', 
    'hash_sistema_inuzaru', 
    1, -- Rol Admin
    true, 
    NOW(),
    NOW()
);