namespace ayalas_street_dogs.Models
{
    public class DetallePlatillo
    {
        public int IdDetallePlatillo { get; set; }

        public int IdPlatillo { get; set; }
        public int IdIngrediente { get; set; }

        public decimal CantidadConsumida { get; set; }

        public virtual Platillo IdPlatilloNavigation { get; set; } = null!;
        public virtual Ingrediente IdIngredienteNavigation { get; set; } = null!;
    }
}
