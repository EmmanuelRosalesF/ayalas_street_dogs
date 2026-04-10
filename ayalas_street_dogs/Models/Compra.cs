using System;
using System.Collections.Generic;

namespace ayalas_street_dogs.Models;

public partial class Compra
{
    public int IdCompra { get; set; }

    public DateTime? FechaC { get; set; }

    public decimal TotalC { get; set; }

    public string Usuario { get; set; } = null!;

    public virtual ICollection<Detallecompra> Detallecompras { get; set; } = new List<Detallecompra>();

    public virtual Usuario UsuarioNavigation { get; set; } = null!;
}
