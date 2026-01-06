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

    public List<SelectListItem> RoleOptions { get; } =
        Enum.GetValues<PersonnelPoste>()
            .Select(r => new SelectListItem(r.ToString(), r.ToString()))
            .ToList();

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

        // map Role (string) -> enum (fallback si valeur inconnue)
        Enum.TryParse<PersonnelPoste>(p.Role, ignoreCase: true, out var poste);

        Form = new PersonnelForm
        {
            Nom = p.Nom,
            Login = p.Login,
            Poste = poste
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // pour ré-afficher correctement la page si erreur
        Found = true;

        if (string.IsNullOrWhiteSpace(Form.Nom))
            ModelState.AddModelError("Form.Nom", "Nom obligatoire.");
        if (string.IsNullOrWhiteSpace(Form.Login))
            ModelState.AddModelError("Form.Login", "Login obligatoire.");

        if (!ModelState.IsValid)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Veuillez corriger les erreurs du formulaire.";
            return Page();
        }

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