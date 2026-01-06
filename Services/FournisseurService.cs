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

                // ✅ FIX: la propriété Telephone n’existe pas dans ton modèle
                // Si plus tard tu ajoutes un champ tel, remplace "" par f.Tel ou f.Phone.
                Phone = "",

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

    public async Task<SupplierRateVm?> GetRatePageAsync(string supplierId)
    {
        if (string.IsNullOrWhiteSpace(supplierId))
            return null;

        var s = await _db.Fournisseurs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == supplierId);

        if (s is null) return null;

        var count = await _db.Commandes.AsNoTracking()
            .CountAsync(c => c.FournisseurId == s.Id);

        return new SupplierRateVm(
            SupplierId: s.Id,
            SupplierName: s.Nom,
            CurrentRating: s.NoteGlobale,
            CurrentRatingsCount: count
        );
    }

    public async Task<ServiceResult> RateAsync(string supplierId, int rating, string? comment)
    {
        if (string.IsNullOrWhiteSpace(supplierId))
            return ServiceResult.Fail("Fournisseur introuvable.");

        if (rating < 1 || rating > 5)
            return ServiceResult.Fail("La note doit être entre 1 et 5.");

        var supplier = await _db.Fournisseurs
            .FirstOrDefaultAsync(x => x.Id == supplierId);

        if (supplier is null)
            return ServiceResult.Fail("Fournisseur introuvable.");

        // MVP : overwrite (comme ton code)
        supplier.NoteGlobale = rating;

        // Commentaire : pour l’instant tu ne le stockes nulle part.
        // Si plus tard tu ajoutes une table FournisseurRating, on le persistera ici.

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}