using System;
using System.Collections.Generic;

namespace ayalas_street_dogs.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public string NombreProveedor { get; set; } = null!;

    public string TelefonoProveedor { get; set; } = null!;

    public string? EmailProveedor { get; set; }

    public string Direccion { get; set; } = null!;

    public int IdCategoria { get; set; }

    public virtual ICollection<Detallecompra> Detallecompras { get; set; } = new List<Detallecompra>();

    public virtual Categorium? IdCategoriaNavigation { get; set; }
}
