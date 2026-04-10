using ayalas_street_dogs.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ayalas_street_dogs.Models;

public partial class Usuario
{
    public string Usuario1 { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; set; } = null!;

    [NotMapped]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarContrasena { get; set; } = null!;

    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }

    public string? EmailUsuario { get; set; }

    public string? TelefonoUsuario { get; set; }

    public string Rol { get; set; } = null!;
    
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
