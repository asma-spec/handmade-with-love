 🌸 Handmade with Love

> Boutique e-commerce d'accessoires artisanaux tunisiens — ASP.NET Core MVC 10

## ✨ Présentation

**Handmade with Love** est une application web e-commerce complète
dédiée à la vente d'accessoires artisanaux faits à la main :
bijoux, bagues, boucles d'oreilles, colliers, serre-têtes,
pochettes et couffins traditionnels tunisiens.

---

## 🚀 Fonctionnalités

### 👁️ Visiteur
- Consulter la page d'accueil avec produits vedettes, nouveautés et promotions réelles
- Parcourir le catalogue avec filtres catégorie, prix et tri
- Voir les fiches produits avec avis clients et produits similaires
- Gérer son panier et appliquer un code promo
- S'inscrire et se connecter

### 🛍️ Client
- Passer une commande avec paiement à la livraison
- Suivre et annuler ses commandes (si statut En attente)
- Gérer sa liste de souhaits (wishlist)
- Programme de fidélité : Bronze / Silver / Gold / Platinum
- Réductions automatiques selon niveau fidélité (5% / 10% / 15%)
- Laisser des avis sur les produits achetés et livrés
- Recevoir des notifications personnalisées basées sur la wishlist
- Chat de support avec bot automatique

### ⚙️ Administrateur
- Tableau de bord : KPIs, CA par mois, commandes par mois, top 5 produits, top 2 catégories, top 3 clients
- Gestion complète des produits avec promotions (%)
- Gestion des commandes et statuts de livraison
- Gestion des utilisateurs et points fidélité
- Modération des avis clients
- Gestion des codes promo (%, montant fixe, livraison gratuite)

---

## 🛠️ Technologies

| Technologie | Usage |
|-------------|-------|
| ASP.NET Core MVC 10 | Framework web principal |
| Entity Framework Core | ORM base de données |
| SQL Server LocalDB | Base de données relationnelle |
| Bootstrap 5 | Interface responsive |
| BCrypt.Net | Hashage des mots de passe |
| Chart.js | Graphiques du dashboard |
| Font Awesome 6 | Icônes |

---

## 📁 Architecture MVC
Handmade-with-Love/
├── Controllers/         # Logique métier
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── CartController.cs
│   ├── ChatController.cs
│   ├── HomeController.cs
│   ├── NotificationController.cs
│   ├── ProductController.cs
│   ├── ShopController.cs
│   └── WishlistController.cs
├── Models/              # Entités base de données
├── ViewModels/          # Modèles formulaires
├── Views/               # Vues Razor .cshtml
├── Data/                # AppDbContext EF Core
├── wwwroot/             # CSS, JS, images
│   ├── css/site.css
│   └── images/
├── Program.cs           # Point d'entrée
└── appsettings.json     # Configuration

---

## ⚙️ Installation

### Prérequis
- Visual Studio 2022+
- .NET 10 SDK
- SQL Server LocalDB

### Étapes

1. **Cloner le projet**
```bash
git clone https://github.com/asma-spec/handmade-with-love.git
cd handmade-with-love
```

2. **Configurer la connexion**

Dans `appsettings.json` :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AccessoryHubDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

3. **Lancer le projet**

Appuyez sur **F5** dans Visual Studio.
La base de données et les données initiales sont créées automatiquement.

### Compte admin par défaut
Email    : admin@gmail.com
Password : admin123

---

## 🎨 Design

- Thème **rose poudré & doré** élégant
- Interface **responsive** (mobile, tablette, desktop)
- Animations CSS et dégradés animés
- Logo circulaire **Handmade with Love**

---

## 📱 Réseaux sociaux

- **Facebook** : [Handmade with Love](https://www.facebook.com/profile.php?id=61575866077900)
- **Instagram** : [@handmadelove667](https://www.instagram.com/handmadelove667/)

---

## 👩‍💻 Auteur

Développé par **Asma** dans le cadre d'un projet académique.

---

*Fait avec ❤️ et passion pour l'artisanat tunisien*
