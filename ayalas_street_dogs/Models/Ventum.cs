using System;
using System.Collections.Generic;

namespace ayalas_street_dogs.Models;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public DateTime? FechaV { get; set; }

    public decimal TotalV { get; set; }

    public string NombreCliente { get; set; } = null!;

    public string Usuario { get; set; } = null!;

    public string Estado { get; set; } = "Pendiente";

    public virtual ICollection<Detalleventum> Detalleventa { get; set; } = new List<Detalleventum>();

    public virtual Usuario UsuarioNavigation { get; set; } = null!;
}
