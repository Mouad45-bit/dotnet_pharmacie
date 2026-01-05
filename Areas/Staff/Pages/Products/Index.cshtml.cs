using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Staff.Pages.Products;

public class IndexModel : PageModel
{
    private readonly IProduitService _produitService;

    public IndexModel(IProduitService produitService) => _produitService = produitService;

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<ProductRow> Rows { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var produits = await _produitService.ListAsync(Query);

        Rows = produits
            .OrderBy(p => p.Nom)
            .Select(p => new ProductRow(
                p.Reference,
                p.Nom,
                p.Prix,
                p.Quantite,
                p.DatePeremption
            ))
            .ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, string? q)
    {
        Query = q;

        var result = await _produitService.DeleteAsync(id);
        if (!result.Success)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = result.Error ?? "Suppression impossible.";
            return RedirectToPage(new { q = Query });
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Produit supprimé.";
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