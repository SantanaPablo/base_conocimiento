using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseConocimiento.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialRefactorV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    categoria_padre_id = table.Column<Guid>(type: "uuid", nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    icono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    es_activa = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                    table.ForeignKey(
                        name: "FK_categorias_categorias_categoria_padre_id",
                        column: x => x.categoria_padre_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_completo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    rol = table.Column<int>(type: "integer", nullable: false),
                    es_activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ultimo_acceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "consultas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pregunta = table.Column<string>(type: "text", nullable: false),
                    respuesta = table.Column<string>(type: "text", nullable: false),
                    conversacion_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resultados_encontrados = table.Column<int>(type: "integer", nullable: false),
                    tiempo_respuesta_ms = table.Column<long>(type: "bigint", nullable: false),
                    fecha_consulta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fue_util = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultas", x => x.id);
                    table.ForeignKey(
                        name: "FK_consultas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manuales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    ruta_storage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    nombre_original = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_subida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    peso_archivo = table.Column<long>(type: "bigint", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    numero_consultas = table.Column<int>(type: "integer", nullable: false),
                    ultima_consulta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manuales", x => x.id);
                    table.ForeignKey(
                        name: "FK_manuales_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_manuales_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "consultas_manuales",
                columns: table => new
                {
                    consulta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manual_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relevancia_promedio = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultas_manuales", x => new { x.consulta_id, x.manual_id });
                    table.ForeignKey(
                        name: "FK_consultas_manuales_consultas_consulta_id",
                        column: x => x.consulta_id,
                        principalTable: "consultas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_consultas_manuales_manuales_manual_id",
                        column: x => x.manual_id,
                        principalTable: "manuales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_categoria_padre_id",
                table: "categorias",
                column: "categoria_padre_id");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                table: "categorias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_consultas_conversacion_id",
                table: "consultas",
                column: "conversacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_usuario_id",
                table: "consultas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_manuales_manual_id",
                table: "consultas_manuales",
                column: "manual_id");

            migrationBuilder.CreateIndex(
                name: "IX_manuales_categoria_id",
                table: "manuales",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_manuales_estado",
                table: "manuales",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_manuales_usuario_id",
                table: "manuales",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consultas_manuales");

            migrationBuilder.DropTable(
                name: "consultas");

            migrationBuilder.DropTable(
                name: "manuales");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
