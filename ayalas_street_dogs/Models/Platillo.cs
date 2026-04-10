using System;
using System.Collections.Generic;

namespace ayalas_street_dogs.Models;

public partial class Platillo
{
    public int IdPlatillo { get; set; }

    public string NombrePlatillo { get; set; } = null!;

    public decimal Precio { get; set; }

    public string? Descripcion { get; set; }

    public int? IdCategoriaMenu { get; set; }

    public virtual ICollection<Detalleventum> Detalleventa { get; set; } = new List<Detalleventum>();

    public virtual ICollection<DetallePlatillo> Detalleplatillo { get; set; } = new List<DetallePlatillo>();

    public virtual CategoriaMenu? IdCategoriaMenuNavigation { get; set; }
}
