namespace Projet.Models
{
    // Géré en session uniquement — pas de table en base
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }
}