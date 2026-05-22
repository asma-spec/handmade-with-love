namespace Projet.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }
        public User? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}