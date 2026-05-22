using System.ComponentModel.DataAnnotations;

namespace Projet.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le nom complet est obligatoire")]
        [Display(Name = "Nom complet")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [Display(Name = "Adresse email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Minimum 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation est obligatoire")]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}