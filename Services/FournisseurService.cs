using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;

namespace project_pharmacie.Services;

public class FournisseurService : IFournisseurService
{
    private readonly PharmacieDbContext _db;
    public FournisseurService(PharmacieDbContext db) => _db = db;

    public async Task<SupplierListVm> GetDashboardAsync(string? sort)
    {
        // 1 requête : fournisseurs + count commandes par fournisseur
        var fournisseurs = await (
            from f in _db.Fournisseurs.AsNoTracking()
            join c in _db.Commandes.AsNoTracking()
                on f.Id equals c.FournisseurId into commandes
            select new
            {
                f.Id,
                Name = f.Nom,
                Phone = f.Telephone,
                Rating = (double)f.NoteGlobale,
                RatingsCount = commandes.Count()
            }
        ).ToListAsync();

        var totalSuppliers = fournisseurs.Count;
        var totalRatingsCount = fournisseurs.Sum(x => x.RatingsCount);
        var averageRating = totalSuppliers == 0 ? 0 : fournisseurs.Average(x => x.Rating);

        var best = fournisseurs
            .OrderByDescending(x => x.Rating)
            .ThenByDescending(x => x.RatingsCount)
            .FirstOrDefault();

        var bestName = best?.Name ?? "-";
        var bestRating = best?.Rating ?? 0;

        var sorted = (sort ?? "rating_desc").ToLowerInvariant() switch
        {
            "rating_asc" => fournisseurs.OrderBy(x => x.Rating).ThenByDescending(x => x.RatingsCount),
            "name" => fournisseurs.OrderBy(x => x.Name),
            _ => fournisseurs.OrderByDescending(x => x.Rating).ThenByDescending(x => x.RatingsCount),
        };

        var rows = sorted.Select(x => new SupplierRowVm(
            x.Id,
            x.Name,
            Phone: string.IsNullOrWhiteSpace(x.Phone) ? "-" : x.Phone!,
            Rating: x.Rating,
            RatingsCount: x.RatingsCount,
            IsPreferred: x.Rating >= 4.5
        )).ToList();

        return new SupplierListVm(
            Rows: rows,
            TotalSuppliers: totalSuppliers,
            AverageRating: averageRating,
            TotalRatingsCount: totalRatingsCount,
            BestSupplierName: bestName,
            BestSupplierRating: bestRating
        );
    }
}