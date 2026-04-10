using System;
using System.Collections.Generic;

namespace ayalas_street_dogs.Models;

public partial class Detalleventum
{
    public int IdVenta { get; set; }

    public int IdPlatillo { get; set; }

    public int CantidadDv { get; set; }

    public decimal CostoDv { get; set; }

    public virtual Platillo? IdPlatilloNavigation { get; set; }

    public virtual Ventum? IdVentaNavigation { get; set; }
}