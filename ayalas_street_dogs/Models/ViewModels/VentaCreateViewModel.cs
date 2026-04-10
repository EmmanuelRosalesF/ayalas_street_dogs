using System.ComponentModel.DataAnnotations;

namespace ayalas_street_dogs.Models.ViewModels
{
    public class VentaCreateViewModel
    {
        public int IdVenta { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Nombre del Cliente")]
        public string NombreCliente { get; set; } = null!;

        [Required]
        public string Usuario { get; set; } = null!;

        public decimal TotalV { get; set; }

        public List<DetalleVentaItem> Detalles { get; set; } = new List<DetalleVentaItem>();
    }

    public class DetalleVentaItem
    {
        [Required(ErrorMessage = "Debe seleccionar un platillo")]
        [Display(Name = "Platillo")]
        public int IdPlatillo { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;

        public string? NombrePlatillo { get; set; }
    }
}
