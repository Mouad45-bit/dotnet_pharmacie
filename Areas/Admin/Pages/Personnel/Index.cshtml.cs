using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class IndexModel : PageModel
{
    private readonly IPersonnelService _service;

    public IndexModel(IPersonnelService service) => _service = service;

    public List<Models.Personnel> Items { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public int Total => Items.Count;

    public async Task OnGetAsync()
    {
        Items = await _service.SearchAsync(Q);
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var res = await _service.DeleteAsync(id);

        TempData["FlashType"] = res.Success ? "success" : "error";
        TempData["FlashMessage"] = res.Success
            ? "Personnel supprimé avec succès."
            : (res.Error ?? "Erreur lors de la suppression.");

        // garder la recherche après delete
        return RedirectToPage(new { q = Q });
    }
}