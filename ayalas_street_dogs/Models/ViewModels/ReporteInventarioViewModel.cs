namespace ayalas_street_dogs.Models.ViewModels
{
    public class ReporteInventarioViewModel
    {
        public DateTime Fecha { get; set; }
        public List<Ingrediente> Ingredientes { get; set; } = new();
        public decimal ValorTotalInventario { get; set; }
    }
}
