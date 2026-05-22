namespace Projet.Models
{
    public class PromoCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "Percentage";
        // Percentage / FixedAmount / FreeShipping
        public decimal Value { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int MaxUsage { get; set; } = 100;
        public int UsageCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}