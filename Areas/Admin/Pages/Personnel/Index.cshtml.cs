using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Areas.Admin.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class IndexModel : PageModel
{
    public List<Services.Personnel> Items { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public int Total => Items.Count;

    public void OnGet()
    {
        Items = PersonnelStore.All(Q);
    }

    public IActionResult OnPostDelete(string id)
    {
        var (ok, error) = PersonnelStore.Delete(id);

        TempData["FlashType"] = ok ? "success" : "error";
        TempData["FlashMessage"] = ok
            ? "Personnel supprimé avec succès."
            : (error ?? "Erreur lors de la suppression.");

        // garder la recherche après delete
        return RedirectToPage(new { q = Q });
    }
}
