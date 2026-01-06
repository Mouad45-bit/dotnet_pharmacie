using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Admin.ViewModels;

public class PersonnelForm
{
    [Required(ErrorMessage = "Nom requis")]
    public string Nom { get; set; } = "";

    [Required(ErrorMessage = "Login requis")]
    public string Login { get; set; } = "";

    [Required(ErrorMessage = "Rôle requis")]
    [RegularExpression("^(ADMIN|PERSONNEL)$", ErrorMessage = "Rôle invalide.")]
    public string Role { get; set; } = "PERSONNEL";
}
