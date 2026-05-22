using Microsoft.EntityFrameworkCore;
using Projet.Models;

namespace Projet.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
        public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
        public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Decimals
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(o => o.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.Discount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.Value)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PromoCode>()
                .Property(p => p.MinOrderAmount)
                .HasColumnType("decimal(18,2)");

            // User → LoyaltyAccount (1-to-1)
            modelBuilder.Entity<User>()
                .HasOne(u => u.LoyaltyAccount)
                .WithOne(l => l.User)
                .HasForeignKey<LoyaltyAccount>(l => l.UserId);

            // Email unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // PromoCode unique
            modelBuilder.Entity<PromoCode>()
                .HasIndex(p => p.Code)
                .IsUnique();
            modelBuilder.Entity<Product>()
               .Property(p => p.DiscountPercent)
               .HasColumnType("decimal(5,2)");
            modelBuilder.Entity<Product>()
               .Property(p => p.DiscountPercent)
               .HasColumnType("decimal(5,2)");
        }
    }
}