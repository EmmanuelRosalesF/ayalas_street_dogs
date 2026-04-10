using System.ComponentModel.DataAnnotations;

namespace ayalas_street_dogs.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario o correo es obligatorio")]
        [Display(Name = "Usuario o Correo Electrónico")]
        public string UsuarioOEmail { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = null!;

        [Display(Name = "Recordarme")]
        public bool RecordarMe { get; set; }
    }
}
