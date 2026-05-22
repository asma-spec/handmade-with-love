using Projet.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ══════════════════════════════════════════
// SEED
// ══════════════════════════════════════════
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    if (!context.Users.Any(u => u.Email == "admin@gmail.com"))
    {
        var admin = new Projet.Models.User
        {
            FullName = "Administrateur",
            Email = "admin@gmail.com",
            Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        context.Users.Add(admin);
        context.SaveChanges();

        context.LoyaltyAccounts.Add(new Projet.Models.LoyaltyAccount
        {
            UserId = admin.Id,
            Points = 0,
            Level = "Platinum"
        });
        context.SaveChanges();
    }

    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Projet.Models.Category
            {
                Name = "Bagues",
                Description = "Bagues et chevalières"
            },
            new Projet.Models.Category
            {
                Name = "Boucles",
                Description = "Boucles d'oreilles"
            },
            new Projet.Models.Category
            {
                Name = "Collier",
                Description = "Colliers et pendentifs"
            },
            new Projet.Models.Category
            {
                Name = "Serre-tête",
                Description = "Serre-têtes et bandeaux"
            },
            new Projet.Models.Category
            {
                Name = "Pochette",
                Description = "Pochettes et petits sacs"
            },
            new Projet.Models.Category
            {
                Name = "Couffin traditionnel",
                Description = "Couffins et paniers"
            },
            new Projet.Models.Category
            {
                Name = "Séries",
                Description = "Collections et séries"
            }
        );
        context.SaveChanges();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erreur seed : {ex.Message}");
}

// ══════════════════════════════════════════
// AJOUT COLONNES
// ══════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();
    try
    {
        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT * FROM sys.columns
                WHERE object_id = OBJECT_ID('Products')
                AND name = 'DiscountPercent'
            )
            ALTER TABLE Products
            ADD DiscountPercent decimal(5,2) NULL");

        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT * FROM sys.columns
                WHERE object_id = OBJECT_ID('Products')
                AND name = 'IsOnSale'
            )
            ALTER TABLE Products
            ADD IsOnSale bit NOT NULL DEFAULT 0");

        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT * FROM sys.tables
                WHERE name = 'Notifications'
            )
            CREATE TABLE Notifications (
                Id       INT IDENTITY(1,1) PRIMARY KEY,
                UserId   INT NOT NULL,
                Title    NVARCHAR(200) NOT NULL,
                Message  NVARCHAR(500) NOT NULL,
                Icon     NVARCHAR(50)  DEFAULT 'fa-bell',
                Link     NVARCHAR(200) DEFAULT '',
                IsRead   BIT           DEFAULT 0,
                CreatedAt DATETIME     DEFAULT GETDATE(),
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            )");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Colonnes : {ex.Message}");
    }
}

app.Run();