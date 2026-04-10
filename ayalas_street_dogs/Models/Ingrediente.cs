namespace ayalas_street_dogs.Models
{
    public class Ingrediente
    {
        public int IdIngrediente { get; set; }

        public string Nombre { get; set; } = null!;

        public decimal StockActual { get; set; }

        public decimal StockMinimo { get; set; }

        public string UnidadMedida { get; set; } = null!;

        public decimal CostoUnitarioPromedio { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime? FechaDesactivacion { get; set; }

        public string? MotivoDesactivacion { get; set; }

        public virtual ICollection<Detallecompra> Detallecompras { get; set; } = new List<Detallecompra>();

        public virtual ICollection<DetallePlatillo> DetallePlatillos { get; set; } = new List<DetallePlatillo>();

        public virtual ICollection<AjusteInventario> AjusteInventarios { get; set; } = new List<AjusteInventario>();
    }
}
