using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Areas.Admin.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    public Services.Personnel? Item { get; private set; }
    public bool Found { get; private set; }

    public void OnGet()
    {
        Item = PersonnelStore.Get(Id);
        Found = Item is not null;
    }

    public IActionResult OnPostDelete(string id)
    {
        var (ok, error) = PersonnelStore.Delete(id);

        TempData["FlashType"] = ok ? "success" : "error";
        TempData["FlashMessage"] = ok
            ? "Personnel supprimé avec succès."
            : (error ?? "Erreur lors de la suppression.");

        return RedirectToPage("/Personnel/Index", new { area = "Admin" });
    }
}
