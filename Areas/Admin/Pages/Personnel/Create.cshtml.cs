using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using project_pharmacie.Areas.Admin.ViewModels;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class CreateModel : PageModel
{
    private readonly IPersonnelService _service;

    public CreateModel(IPersonnelService service) => _service = service;

    [BindProperty]
    public PersonnelForm Form { get; set; } = new();

    // ADMIN / PERSONNEL
    public List<SelectListItem> RoleOptions { get; } = new()
    {
        new SelectListItem("Personnel", "PERSONNEL"),
        new SelectListItem("Admin", "ADMIN")
    };

    public void OnGet()
    {
        // valeur par défaut
        Form.Role = "PERSONNEL";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Form.Nom))
            ModelState.AddModelError("Form.Nom", "Nom obligatoire.");
        if (string.IsNullOrWhiteSpace(Form.Login))
            ModelState.AddModelError("Form.Login", "Login obligatoire.");

        // validation du rôle
        var role = (Form.Role ?? "").Trim().ToUpperInvariant();
        if (role is not ("ADMIN" or "PERSONNEL"))
            ModelState.AddModelError("Form.Role", "Rôle invalide (ADMIN ou PERSONNEL).");

        if (!ModelState.IsValid)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Veuillez corriger les erreurs du formulaire.";
            return Page();
        }

        // on normalise
        Form.Role = role;

        var res = await _service.CreateAsync(Form);

        if (!res.Success)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = res.Error ?? "Erreur lors de la création.";
            return Page();
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Personnel ajouté avec succès.";
        return RedirectToPage("/Personnel/Index", new { area = "Admin" });
    }
}