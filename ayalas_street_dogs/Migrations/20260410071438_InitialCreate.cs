using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ayalas_street_dogs.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    idCategoria = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombreCategoria = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo = table.Column<string>(type: "enum('Producto','Servicio')", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idCategoria);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "categoriaMenu",
                columns: table => new
                {
                    idCategoriaMenu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombreCategoriaMenu = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ordenVisualizacion = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    icono = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "basket", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoriaMenu", x => x.idCategoriaMenu);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "ingrediente",
                columns: table => new
                {
                    IdIngrediente = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StockActual = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    UnidadMedida = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostoUnitarioPromedio = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    FechaDesactivacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MotivoDesactivacion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.IdIngrediente);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    usuario = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contrasena = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombres = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    apellidos = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    emailUsuario = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefonoUsuario = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rol = table.Column<string>(type: "enum('admin','empleado','cliente')", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.usuario);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "proveedor",
                columns: table => new
                {
                    idProveedor = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombreProveedor = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefonoProveedor = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    emailProveedor = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    direccion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    idCategoria = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idProveedor);
                    table.ForeignKey(
                        name: "FK_Proveedor_Categoria",
                        column: x => x.idCategoria,
                        principalTable: "categoria",
                        principalColumn: "idCategoria",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "platillo",
                columns: table => new
                {
                    idPlatillo = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombrePlatillo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    precio = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    idCategoriaMenu = table.Column<int>(type: "int(11)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idPlatillo);
                    table.ForeignKey(
                        name: "FK_Platillo_CategoriaMenu",
                        column: x => x.idCategoriaMenu,
                        principalTable: "categoriaMenu",
                        principalColumn: "idCategoriaMenu",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "ajusteinventario",
                columns: table => new
                {
                    IdAjuste = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdIngrediente = table.Column<int>(type: "int(11)", nullable: false),
                    FechaAjuste = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cantidad = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    Motivo = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Usuario = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.IdAjuste);
                    table.ForeignKey(
                        name: "FK_AjusteInventario_Ingrediente",
                        column: x => x.IdIngrediente,
                        principalTable: "ingrediente",
                        principalColumn: "IdIngrediente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "compra",
                columns: table => new
                {
                    idCompra = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fechaC = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp()"),
                    totalC = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    usuario = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idCompra);
                    table.ForeignKey(
                        name: "FK_Compra_Usuario",
                        column: x => x.usuario,
                        principalTable: "usuario",
                        principalColumn: "usuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "venta",
                columns: table => new
                {
                    idVenta = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fechaV = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "current_timestamp()"),
                    totalV = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    nombreCliente = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usuario = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente", collation: "utf8mb4_general_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.idVenta);
                    table.ForeignKey(
                        name: "FK_Venta_Usuario",
                        column: x => x.usuario,
                        principalTable: "usuario",
                        principalColumn: "usuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "detalleplatillo",
                columns: table => new
                {
                    IdDetallePlatillo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdPlatillo = table.Column<int>(type: "int(11)", nullable: false),
                    IdIngrediente = table.Column<int>(type: "int(11)", nullable: false),
                    CantidadConsumida = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.IdDetallePlatillo);
                    table.ForeignKey(
                        name: "FK_DetallePlatillo_Ingrediente",
                        column: x => x.IdIngrediente,
                        principalTable: "ingrediente",
                        principalColumn: "IdIngrediente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallePlatillo_Platillo",
                        column: x => x.IdPlatillo,
                        principalTable: "platillo",
                        principalColumn: "idPlatillo",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "detallecompra",
                columns: table => new
                {
                    idCompra = table.Column<int>(type: "int(11)", nullable: false),
                    idIngrediente = table.Column<int>(type: "int(11)", nullable: false),
                    idProveedor = table.Column<int>(type: "int(11)", nullable: false),
                    cantidadDC = table.Column<int>(type: "int", precision: 10, scale: 3, nullable: false),
                    costoDC = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.idCompra, x.idIngrediente })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_DetalleCompra_Compra",
                        column: x => x.idCompra,
                        principalTable: "compra",
                        principalColumn: "idCompra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleCompra_Ingrediente",
                        column: x => x.idIngrediente,
                        principalTable: "ingrediente",
                        principalColumn: "IdIngrediente");
                    table.ForeignKey(
                        name: "FK_DetalleCompra_Proveedor",
                        column: x => x.idProveedor,
                        principalTable: "proveedor",
                        principalColumn: "idProveedor");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "detalleventa",
                columns: table => new
                {
                    idVenta = table.Column<int>(type: "int(11)", nullable: false),
                    idPlatillo = table.Column<int>(type: "int(11)", nullable: false),
                    cantidadDV = table.Column<int>(type: "int(11)", nullable: false),
                    costoDV = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.idVenta, x.idPlatillo })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_DetalleVenta_Platillo",
                        column: x => x.idPlatillo,
                        principalTable: "platillo",
                        principalColumn: "idPlatillo");
                    table.ForeignKey(
                        name: "FK_DetalleVenta_Venta",
                        column: x => x.idVenta,
                        principalTable: "venta",
                        principalColumn: "idVenta",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ajusteinventario_IdIngrediente",
                table: "ajusteinventario",
                column: "IdIngrediente");

            migrationBuilder.CreateIndex(
                name: "FK_Compra_Usuario",
                table: "compra",
                column: "usuario");

            migrationBuilder.CreateIndex(
                name: "FK_DetalleCompra_Ingrediente",
                table: "detallecompra",
                column: "idIngrediente");

            migrationBuilder.CreateIndex(
                name: "FK_DetalleCompra_Proveedor",
                table: "detallecompra",
                column: "idProveedor");

            migrationBuilder.CreateIndex(
                name: "IX_detalleplatillo_IdIngrediente",
                table: "detalleplatillo",
                column: "IdIngrediente");

            migrationBuilder.CreateIndex(
                name: "IX_detalleplatillo_IdPlatillo",
                table: "detalleplatillo",
                column: "IdPlatillo");

            migrationBuilder.CreateIndex(
                name: "FK_DetalleVenta_Platillo",
                table: "detalleventa",
                column: "idPlatillo");

            migrationBuilder.CreateIndex(
                name: "FK_Platillo_CategoriaMenu",
                table: "platillo",
                column: "idCategoriaMenu");

            migrationBuilder.CreateIndex(
                name: "emailProveedor",
                table: "proveedor",
                column: "emailProveedor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "FK_Proveedor_Categoria",
                table: "proveedor",
                column: "idCategoria");

            migrationBuilder.CreateIndex(
                name: "emailUsuario",
                table: "usuario",
                column: "emailUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "telefonoUsuario",
                table: "usuario",
                column: "telefonoUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "FK_Venta_Usuario",
                table: "venta",
                column: "usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ajusteinventario");

            migrationBuilder.DropTable(
                name: "detallecompra");

            migrationBuilder.DropTable(
                name: "detalleplatillo");

            migrationBuilder.DropTable(
                name: "detalleventa");

            migrationBuilder.DropTable(
                name: "compra");

            migrationBuilder.DropTable(
                name: "proveedor");

            migrationBuilder.DropTable(
                name: "ingrediente");

            migrationBuilder.DropTable(
                name: "platillo");

            migrationBuilder.DropTable(
                name: "venta");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "categoriaMenu");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
