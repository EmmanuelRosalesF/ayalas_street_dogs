namespace ayalas_street_dogs.Models
{
    public class CategoriaMenu
    {
        public int IdCategoriaMenu { get; set; }

        public string NombreCategoriaMenu { get; set; } = null!;

        public int OrdenVisualizacion { get; set; }

        public string Icono { get; set; } = "basket";

        // Relación con Platillos
        public virtual ICollection<Platillo> Platillos { get; set; } = new List<Platillo>();
    }
}
