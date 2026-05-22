namespace Projet.Models
{
    public class LoyaltyTransaction
    {
        public int Id { get; set; }
        public int Points { get; set; }
        public string Type { get; set; } = "Crédit"; // Crédit / Débit
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int LoyaltyAccountId { get; set; }
        public LoyaltyAccount? LoyaltyAccount { get; set; }
    }
}