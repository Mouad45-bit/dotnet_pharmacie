using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Clients;

public class IndexModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public IndexModel(PharmacieDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public List<Client> Clients { get; private set; } = new();
    public int Total { get; private set; }

    public async Task OnGetAsync()
    {
        var query = _db.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var q = Q.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.Name, $"%{q}%") ||
                EF.Functions.Like(c.Email, $"%{q}%")
            );
        }

        Clients = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        Total = Clients.Count;
    }

    // Handler appelé par: <form method="post" asp-page-handler="Delete">
    public async Task<IActionResult> OnPostDeleteAsync(string id, string? q)
    {
        Q = q;

        var client = await _db.Clients.FindAsync(id);
        if (client is null)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Client introuvable.";
            return RedirectToPage(new { q = Q });
        }

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = $"Client supprimé : {client.Name}.";
        return RedirectToPage(new { q = Q });
    }
}
