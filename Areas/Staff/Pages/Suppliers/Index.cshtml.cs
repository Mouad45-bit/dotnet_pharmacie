using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;

namespace project_pharmacie.Areas.Staff.Pages.Suppliers;

public class IndexModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public IndexModel(PharmacieDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; } = "rating_desc";

    public List<SupplierRow> Rows { get; private set; } = new();

    public int TotalSuppliers { get; private set; }
    public double AverageRating { get; private set; }
    public int TotalRatingsCount { get; private set; }
    public string BestSupplierName { get; private set; } = "-";
    public double BestSupplierRating { get; private set; }

    public async Task OnGetAsync()
    {
        var fournisseurs = await _db.Fournisseurs
            .AsNoTracking()
            .Select(f => new
            {
                f.Id,
                Name = f.Nom,
                Rating = f.NoteGlobale,
                RatingsCount = _db.Commandes.Count(c => c.FournisseurId == f.Id)
            })
            .ToListAsync();

        TotalSuppliers = fournisseurs.Count;
        TotalRatingsCount = fournisseurs.Sum(x => x.RatingsCount);
        AverageRating = fournisseurs.Count == 0 ? 0 : fournisseurs.Average(x => x.Rating);

        var best = fournisseurs
            .OrderByDescending(x => x.Rating)
            .ThenByDescending(x => x.RatingsCount)
            .FirstOrDefault();

        if (best is not null)
        {
            BestSupplierName = best.Name;
            BestSupplierRating = best.Rating;
        }

        var sorted = Sort?.ToLowerInvariant() switch
        {
            "rating_asc" => fournisseurs.OrderBy(x => x.Rating).ThenByDescending(x => x.RatingsCount),
            "name" => fournisseurs.OrderBy(x => x.Name),
            _ => fournisseurs.OrderByDescending(x => x.Rating).ThenByDescending(x => x.RatingsCount),
        };

        Rows = sorted.Select(x => new SupplierRow(
            x.Id,
            x.Name,
            Phone: "-",
            Rating: x.Rating,
            RatingsCount: x.RatingsCount,
            IsPreferred: x.Rating >= 4.5
        )).ToList();
    }

    public (string badgeCls, string text) GetRatingBadge(double rating)
    {
        if (rating >= 4.5)
            return ("bg-emerald-50 text-emerald-700 ring-emerald-200", "Excellent");
        if (rating >= 3.5)
            return ("bg-amber-50 text-amber-700 ring-amber-200", "Bon");
        return ("bg-red-50 text-red-700 ring-red-200", "À surveiller");
    }

    public record SupplierRow(
        string Id,
        string Name,
        string Phone,
        double Rating,
        int RatingsCount,
        bool IsPreferred
    );
}