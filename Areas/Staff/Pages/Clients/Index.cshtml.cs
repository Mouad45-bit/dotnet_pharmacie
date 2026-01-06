using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Staff.Pages.Clients;

public class IndexModel : PageModel
{
    private readonly IClientService _clients;
    public IndexModel(IClientService clients) => _clients = clients;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public List<Client> Clients { get; private set; } = new();
    public int Total { get; private set; }

    public async Task OnGetAsync()
    {
        (Clients, Total) = await _clients.SearchAsync(Q);
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, string? q)
    {
        Q = q;

        var res = await _clients.DeleteAsync(id);
        TempData["FlashType"] = res.Success ? "success" : "error";
        TempData["FlashMessage"] = res.Success ? "Client supprimé." : (res.Error ?? "Erreur.");

        return RedirectToPage(new { q = Q });
    }
}