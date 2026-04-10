using System;
using System.Collections.Generic;

namespace ayalas_street_dogs.Models;

public partial class Detallecompra
{
    public int IdCompra { get; set; }

    public int idIngrediente { get; set; }
    public int IdProveedor { get; set; }

    public int CantidadDc { get; set; }

    public decimal CostoDc { get; set; }

    public virtual Compra IdCompraNavigation { get; set; } = null!;

    public virtual Ingrediente IdIngredienteNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
