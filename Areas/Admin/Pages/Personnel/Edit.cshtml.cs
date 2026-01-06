using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using project_pharmacie.Areas.Admin.ViewModels;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class EditModel : PageModel
{
    private readonly IPersonnelService _service;

    public EditModel(IPersonnelService service) => _service = service;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    [BindProperty]
    public PersonnelForm Form { get; set; } = new();

    public bool Found { get; private set; }

    // ADMIN / PERSONNEL
    public List<SelectListItem> RoleOptions { get; } = new()
    {
        new SelectListItem("Personnel", "PERSONNEL"),
        new SelectListItem("Admin", "ADMIN")
    };

    public async Task<IActionResult> OnGetAsync()
    {
        var p = await _service.GetAsync(Id);
        if (p is null)
        {
            Found = false;
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Personnel introuvable.";
            return Page();
        }

        Found = true;

        // On garde uniquement ADMIN/PERSONNEL (fallback -> PERSONNEL)
        var role = (p.Role ?? "").Trim().ToUpperInvariant();
        if (role is not ("ADMIN" or "PERSONNEL"))
            role = "PERSONNEL";

        Form = new PersonnelForm
        {
            Nom = p.Nom,
            Login = p.Login,
            Role = role
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Found = true;

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

        // on normalise avant d'envoyer au service
        Form.Role = role;

        var res = await _service.UpdateAsync(Id, Form);

        if (!res.Success)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = res.Error ?? "Erreur lors de la mise à jour.";
            return Page();
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Modifications enregistrées.";
        return RedirectToPage("/Personnel/Index", new { area = "Admin" });
    }
}