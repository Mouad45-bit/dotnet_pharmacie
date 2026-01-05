using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;

namespace project_pharmacie.Areas.Staff.Pages.Products;

public class IndexModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public IndexModel(PharmacieDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<ProductRow> Rows { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var q = _db.Produits.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Query))
        {
            var term = Query.Trim();
            q = q.Where(p =>
                EF.Functions.Like(p.Nom, $"%{term}%") ||
                EF.Functions.Like(p.Reference, $"%{term}%")
            );
        }

        Rows = await q
            .OrderBy(p => p.Nom)
            .Select(p => new ProductRow(
                p.Reference,
                p.Nom,
                p.Prix,
                p.Quantite,
                p.DatePeremption
            ))
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, string? q)
    {
        Query = q;

        var prod = await _db.Produits.FindAsync(id);
        if (prod is null)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Produit introuvable.";
            return RedirectToPage(new { q = Query });
        }

        _db.Produits.Remove(prod);
        await _db.SaveChangesAsync();

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = $"Produit supprimé : {prod.Nom}.";
        return RedirectToPage(new { q = Query });
    }

    public (string badgeCls, string text) GetStockBadge(int stock)
    {
        if (stock == 0)
            return ("bg-red-50 text-red-700 ring-red-200", "Rupture");

        if (stock <= 5)
            return ("bg-amber-50 text-amber-700 ring-amber-200", "Alerte");

        return ("bg-emerald-50 text-emerald-700 ring-emerald-200", "OK");
    }

    public record ProductRow(
        string Reference,
        string Nom,
        decimal Prix,
        int Quantite,
        DateTime? DatePeremption
    );
}