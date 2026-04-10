using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ayalas_street_dogs.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categorium> Categoria { get; set; }
    public virtual DbSet<CategoriaMenu> CategoriaMenus { get; set; }
    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<Detallecompra> Detallecompras { get; set; }

    public virtual DbSet<Detalleventum> Detalleventa { get; set; }

    public virtual DbSet<Platillo> Platillos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    public DbSet<Ingrediente> Ingrediente { get; set; }
    public DbSet<DetallePlatillo> DetallePlatillo { get; set; }
    public DbSet<AjusteInventario> AjusteInventario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PRIMARY");

            entity.ToTable("categoria");

            entity.Property(e => e.IdCategoria)
                .HasColumnType("int(11)")
                .HasColumnName("idCategoria");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(50)
                .HasColumnName("nombreCategoria");
            entity.Property(e => e.Tipo)
                .HasColumnType("enum('Producto','Servicio')")
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra).HasName("PRIMARY");

            entity.ToTable("compra");

            entity.HasIndex(e => e.Usuario, "FK_Compra_Usuario");

            entity.Property(e => e.IdCompra)
                .HasColumnType("int(11)")
                .HasColumnName("idCompra");
            entity.Property(e => e.FechaC)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("fechaC");
            entity.Property(e => e.TotalC)
                .HasPrecision(10, 2)
                .HasColumnName("totalC");
            entity.Property(e => e.Usuario)
                .HasMaxLength(15)
                .HasColumnName("usuario");

            entity.HasOne(d => d.UsuarioNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.Usuario)
                .HasConstraintName("FK_Compra_Usuario");
        });

        modelBuilder.Entity<Detallecompra>(entity =>
        {
            // 🛑 1. CLAVE PRIMARIA COMPUESTA CORREGIDA: (IdCompra, IdIngrediente)
            // El nombre del campo 'idIngrediente' en el modelo C# es CamelCase, ajusta aquí si es necesario.
            entity.HasKey(e => new { e.IdCompra, e.idIngrediente })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("detallecompra");

            // Asegúrate de que este índice aún sea necesario si lo estás usando.
            entity.HasIndex(e => e.IdProveedor, "FK_DetalleCompra_Proveedor");

            // 🛑 NUEVO ÍNDICE PARA LA CLAVE FORÁNEA DE INGREDIENTE
            entity.HasIndex(e => e.idIngrediente, "FK_DetalleCompra_Ingrediente");

            // Mapeo de Propiedades
            entity.Property(e => e.IdCompra)
                .HasColumnType("int(11)")
                .HasColumnName("idCompra");

            // 🛑 NUEVO: Mapeo de IdIngrediente
            entity.Property(e => e.idIngrediente)
                .HasColumnType("int(11)")
                .HasColumnName("idIngrediente");

            entity.Property(e => e.IdProveedor)
                .HasColumnType("int(11)")
                .HasColumnName("idProveedor");

            // 🛑 CORRECCIÓN: CantidadDC debe ser DECIMAL(10,3) para inventario fraccionario
            entity.Property(e => e.CantidadDc)
                .HasPrecision(10, 3) // Decimales para manejar kilos o litros
                .HasColumnName("cantidadDC");

            entity.Property(e => e.CostoDc)
                .HasPrecision(10, 2)
                .HasColumnName("costoDC");

            // Mapeo de Relaciones
            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.Detallecompras)
                .HasForeignKey(d => d.IdCompra)
                .HasConstraintName("FK_DetalleCompra_Compra");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Detallecompras)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Proveedor");

            entity.HasOne(d => d.IdIngredienteNavigation).WithMany(p => p.Detallecompras)
                .HasForeignKey(d => d.idIngrediente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCompra_Ingrediente");
        });

        modelBuilder.Entity<Detalleventum>(entity =>
        {
            entity.HasKey(e => new { e.IdVenta, e.IdPlatillo })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("detalleventa");

            entity.HasIndex(e => e.IdPlatillo, "FK_DetalleVenta_Platillo");

            entity.Property(e => e.IdVenta)
                .HasColumnType("int(11)")
                .HasColumnName("idVenta");
            entity.Property(e => e.IdPlatillo)
                .HasColumnType("int(11)")
                .HasColumnName("idPlatillo");
            entity.Property(e => e.CantidadDv)
                .HasColumnType("int(11)")
                .HasColumnName("cantidadDV");
            entity.Property(e => e.CostoDv)
                .HasPrecision(10, 2)
                .HasColumnName("costoDV");

            entity.HasOne(d => d.IdPlatilloNavigation).WithMany(p => p.Detalleventa)
                .HasForeignKey(d => d.IdPlatillo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_Platillo");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.Detalleventa)
                .HasForeignKey(d => d.IdVenta)
                .HasConstraintName("FK_DetalleVenta_Venta");
        });

        modelBuilder.Entity<Platillo>(entity =>
        {
            entity.HasKey(e => e.IdPlatillo).HasName("PRIMARY");

            entity.ToTable("platillo");

            entity.HasIndex(e => e.IdCategoriaMenu, "FK_Platillo_CategoriaMenu");

            entity.Property(e => e.IdPlatillo)
                .HasColumnType("int(11)")
                .HasColumnName("idPlatillo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdCategoriaMenu)
                .HasColumnType("int(11)")
                .HasColumnName("idCategoriaMenu");
            entity.Property(e => e.NombrePlatillo)
                .HasMaxLength(50)
                .HasColumnName("nombrePlatillo");
            entity.Property(e => e.Precio)
                .HasPrecision(10, 2)
                .HasColumnName("precio");

            entity.HasOne(d => d.IdCategoriaMenuNavigation).WithMany(p => p.Platillos)
                .HasForeignKey(d => d.IdCategoriaMenu)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Platillo_CategoriaMenu");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PRIMARY");

            entity.ToTable("proveedor");

            entity.HasIndex(e => e.IdCategoria, "FK_Proveedor_Categoria");

            entity.HasIndex(e => e.EmailProveedor, "emailProveedor").IsUnique();

            entity.Property(e => e.IdProveedor)
                .HasColumnType("int(11)")
                .HasColumnName("idProveedor");
            entity.Property(e => e.Direccion)
                .HasMaxLength(50)
                .HasColumnName("direccion");
            entity.Property(e => e.EmailProveedor)
                .HasMaxLength(100)
                .HasColumnName("emailProveedor");
            entity.Property(e => e.IdCategoria)
                .HasColumnType("int(11)")
                .HasColumnName("idCategoria");
            entity.Property(e => e.NombreProveedor)
                .HasMaxLength(50)
                .HasColumnName("nombreProveedor");
            entity.Property(e => e.TelefonoProveedor)
                .HasMaxLength(20)
                .HasColumnName("telefonoProveedor");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Proveedors)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK_Proveedor_Categoria");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Usuario1).HasName("PRIMARY");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.EmailUsuario, "emailUsuario").IsUnique();

            entity.HasIndex(e => e.TelefonoUsuario, "telefonoUsuario").IsUnique();

            entity.Property(e => e.Usuario1)
                .HasMaxLength(15)
                .HasColumnName("usuario");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .HasColumnName("apellidos");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(100)
                .HasColumnName("contrasena");
            entity.Property(e => e.EmailUsuario)
                .HasMaxLength(100)
                .HasColumnName("emailUsuario");
            entity.Property(e => e.Nombres)
                .HasMaxLength(50)
                .HasColumnName("nombres");
            entity.Property(e => e.Rol)
                .HasColumnType("enum('admin','empleado','cliente')")
                .HasColumnName("rol");
            entity.Property(e => e.TelefonoUsuario)
                .HasMaxLength(20)
                .HasColumnName("telefonoUsuario");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PRIMARY");

            entity.ToTable("venta");

            entity.HasIndex(e => e.Usuario, "FK_Venta_Usuario");

            entity.Property(e => e.IdVenta)
                .HasColumnType("int(11)")
                .HasColumnName("idVenta");
            entity.Property(e => e.FechaV)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("datetime")
                .HasColumnName("fechaV");
            entity.Property(e => e.NombreCliente)
                .HasMaxLength(50)
                .HasColumnName("nombreCliente");
            entity.Property(e => e.TotalV)
                .HasPrecision(10, 2)
                .HasColumnName("totalV");
            entity.Property(e => e.Usuario)
                .HasMaxLength(15)
                .HasColumnName("usuario");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasColumnName("estado")
                .HasDefaultValue("Pendiente");
            entity.HasOne(d => d.UsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.Usuario)
                .HasConstraintName("FK_Venta_Usuario");
        });

        modelBuilder.Entity<CategoriaMenu>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaMenu);

            entity.ToTable("categoriaMenu");

            entity.Property(e => e.IdCategoriaMenu)
                .HasColumnName("idCategoriaMenu");

            entity.Property(e => e.NombreCategoriaMenu)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("nombreCategoriaMenu");

            entity.Property(e => e.OrdenVisualizacion)
                .HasColumnName("ordenVisualizacion")
                .HasDefaultValue(0);

            entity.Property(e => e.Icono)
                .HasMaxLength(50)
                .HasColumnName("icono")
                .HasDefaultValue("basket");
        });

        modelBuilder.Entity<Ingrediente>(entity =>
        {
            entity.HasKey(e => e.IdIngrediente).HasName("PRIMARY");

            entity.ToTable("ingrediente");

            // 🛑 Mapeo de precisión decimal para Stock
            entity.Property(e => e.StockActual)
                .HasPrecision(10, 3)
                .HasColumnName("StockActual");

            entity.Property(e => e.StockMinimo)
                .HasPrecision(10, 3)
                .HasColumnName("StockMinimo");

            entity.Property(e => e.CostoUnitarioPromedio)
                .HasPrecision(10, 4) // Manteniendo 4 decimales para precisión de costo
                .HasColumnName("CostoUnitarioPromedio");

            entity.Property(e => e.IdIngrediente)
                .HasColumnType("int(11)")
                .HasColumnName("IdIngrediente");

            entity.Property(e => e.Activo)
        .HasColumnName("Activo")
        .HasDefaultValue(true);

            entity.Property(e => e.FechaDesactivacion)
                .HasColumnName("FechaDesactivacion");

            entity.Property(e => e.MotivoDesactivacion)
                .HasMaxLength(200)
                .HasColumnName("MotivoDesactivacion");

            // (Añade aquí cualquier otra configuración de columna si es necesario)
        });

        modelBuilder.Entity<DetallePlatillo>(entity =>
        {
            entity.HasKey(e => e.IdDetallePlatillo).HasName("PRIMARY");

            entity.ToTable("detalleplatillo");

            entity.Property(e => e.CantidadConsumida)
                .HasPrecision(10, 3)
                .HasColumnName("CantidadConsumida");

            entity.HasOne(d => d.IdPlatilloNavigation).WithMany(p => p.Detalleplatillo)
                .HasForeignKey(d => d.IdPlatillo)
                .HasConstraintName("FK_DetallePlatillo_Platillo");

            entity.HasOne(d => d.IdIngredienteNavigation).WithMany(p => p.DetallePlatillos)
                .HasForeignKey(d => d.IdIngrediente)
                .HasConstraintName("FK_DetallePlatillo_Ingrediente");
        });

        modelBuilder.Entity<AjusteInventario>(entity =>
        {
            entity.HasKey(e => e.IdAjuste).HasName("PRIMARY");

            entity.ToTable("ajusteinventario");

            entity.Property(e => e.Cantidad)
                .HasPrecision(10, 3)
                .HasColumnName("Cantidad");

            entity.HasOne(d => d.IdIngredienteNavigation).WithMany(p => p.AjusteInventarios)
                .HasForeignKey(d => d.IdIngrediente)
                .HasConstraintName("FK_AjusteInventario_Ingrediente");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
