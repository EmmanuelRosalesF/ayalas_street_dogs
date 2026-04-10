namespace ayalas_street_dogs.Models
{
    public class AjusteInventario
    {
        public int IdAjuste { get; set; }

        public int IdIngrediente { get; set; }

        public DateTime FechaAjuste { get; set; }
        public string TipoMovimiento { get; set; } = null!;
        public decimal Cantidad { get; set; }
        public string? Motivo { get; set; }
        public string Usuario { get; set; } = null!;

        public virtual Ingrediente IdIngredienteNavigation { get; set; } = null!;
    }
}
