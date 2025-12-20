using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Areas.Admin.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class IndexModel : PageModel
{
    public List<Services.Personnel> Items { get; private set; } = new();

    public void OnGet()
    {
        Items = PersonnelStore.All();
    }

    public IActionResult OnPostDelete(string id)
    {
        var (ok, error) = PersonnelStore.Delete(id);

        if (!ok)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = error ?? "Erreur lors de la suppression.";
            return RedirectToPage();
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Personnel supprimé avec succès.";
        return RedirectToPage();
    }
}