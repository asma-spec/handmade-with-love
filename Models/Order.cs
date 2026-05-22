namespace Projet.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Discount { get; set; } = 0;
        public string Status { get; set; } = "En attente";
        // En attente / Confirmée / En préparation / Expédiée / Livrée / Annulée
        public string? TrackingNumber { get; set; }
        public string? PromoCodeUsed { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}