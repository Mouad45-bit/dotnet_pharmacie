using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using project_pharmacie.Areas.Admin.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class EditModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    [BindProperty]
    public PersonnelForm Form { get; set; } = new();

    public bool Found { get; private set; }

    public List<SelectListItem> RoleOptions { get; } =
        Enum.GetValues<PersonnelRole>()
            .Select(r => new SelectListItem(r.ToString(), r.ToString()))
            .ToList();

    public IActionResult OnGet()
    {
        var p = PersonnelStore.Get(Id);
        if (p is null)
        {
            Found = false;
            return Page();
        }

        Found = true;
        Form = new PersonnelForm
        {
            FullName = p.FullName,
            Email = p.Email,
            Phone = p.Phone,
            Role = p.Role,
            IsActive = p.IsActive
        };

        return Page();
    }

    public IActionResult OnPost()
    {
        var existing = PersonnelStore.Get(Id);
        if (existing is null)
        {
            Found = false;
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Personnel introuvable.";
            return Page();
        }

        Found = true;

        if (string.IsNullOrWhiteSpace(Form.FullName))
            ModelState.AddModelError("Form.FullName", "Nom complet obligatoire.");
        if (string.IsNullOrWhiteSpace(Form.Email))
            ModelState.AddModelError("Form.Email", "Email obligatoire.");

        if (!ModelState.IsValid)
            return Page();

        var (ok, error) = PersonnelStore.Update(Id, Form);

        if (!ok)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = error ?? "Erreur lors de la mise à jour.";
            return Page();
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Modifications enregistrées.";
        return RedirectToPage("/Personnel/Index", new { area = "Admin" });
    }
}