namespace Projet.Models
{
    public class LoyaltyAccount
    {
        public int Id { get; set; }
        public int Points { get; set; } = 0;
        public string Level { get; set; } = "Bronze";
        // Bronze(0-999) / Silver(1000-4999) / Gold(5000-9999) / Platinum(10000+)

        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
    }
}