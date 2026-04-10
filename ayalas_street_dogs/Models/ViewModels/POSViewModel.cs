using static ayalas_street_dogs.Controllers.HomeController;

namespace ayalas_street_dogs.Models.ViewModels
{
    // ViewModel principal para la vista del POS
    public class POSViewModel
    {
        public List<CategoriaMenu> Categorias { get; set; } = new();
        public List<Platillo> Platillos { get; set; } = new();
        public List<VentaRecienteDto> VentasRecientes { get; set; } = new();
        public List<VentaRecienteDto> VentasPendientes { get; set; } = new();
    }

    // Request para crear una venta desde el POS
    public class POSVentaRequest
    {
        public string? NombreCliente { get; set; }
        public List<POSItemRequest> Items { get; set; } = new();
    }

    // Item individual del carrito
    public class POSItemRequest
    {
        public int IdPlatillo { get; set; }
        public int Cantidad { get; set; }
    }

    public class VentaRecienteDto
    {
        public int IdVenta { get; set; }
        public string NombreCliente { get; set; } = null!;
        public DateTime? FechaV { get; set; }
        public decimal TotalV { get; set; }
        public int CantidadProductos { get; set; }
        public string PrimerProducto { get; set; } = null!;
        public string Estado { get; set; } = null!;
    }

}