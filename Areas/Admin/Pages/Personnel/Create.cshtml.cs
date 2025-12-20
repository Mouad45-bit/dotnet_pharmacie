using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using project_pharmacie.Areas.Admin.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class CreateModel : PageModel
{
    [BindProperty]
    public PersonnelForm Form { get; set; } = new();

    [TempData]
    public string? FormError { get; set; } // message général

    public List<SelectListItem> RoleOptions { get; } =
        Enum.GetValues<PersonnelRole>()
            .Select(r => new SelectListItem(r.ToString(), r.ToString()))
            .ToList();

    public void OnGet()
    {
        // default
        if (Form.Role == 0) Form.Role = PersonnelRole.Pharmacien;
        Form.IsActive = true;
    }

    public IActionResult OnPost()
    {
        // validations minimales côté page
        if (string.IsNullOrWhiteSpace(Form.FullName))
            ModelState.AddModelError("Form.FullName", "Nom complet obligatoire.");
        if (string.IsNullOrWhiteSpace(Form.Email))
            ModelState.AddModelError("Form.Email", "Email obligatoire.");

        if (!ModelState.IsValid)
            return Page();

        var (ok, error, _) = PersonnelStore.Add(Form);

        if (!ok)
        {
            // erreur générale (ex: email déjà utilisé)
            FormError = error ?? "Erreur lors de la création.";
            return Page();
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Personnel ajouté avec succès.";
        return RedirectToPage("/Personnel/Index", new { area = "Admin" });
    }
}