using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class ProduitService : IProduitService
{
    private readonly PharmacieDbContext _db;

    public ProduitService(PharmacieDbContext db) => _db = db;

    public async Task<List<Produit>> ListAsync(string? query = null)
    {
        var q = _db.Produits.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(p =>
                EF.Functions.Like(p.Nom, $"%{term}%") ||
                EF.Functions.Like(p.Reference, $"%{term}%")
            );
        }

        return await q.OrderBy(p => p.Nom).ToListAsync();
    }

    public Task<Produit?> GetByReferenceAsync(string reference)
        => _db.Produits.FirstOrDefaultAsync(p => p.Reference == reference);

    public async Task<ServiceResult> DecreaseStockAsync(string reference, int quantity)
    {
        if (quantity <= 0) return ServiceResult.Fail("Quantité invalide.");

        var product = await _db.Produits.FirstOrDefaultAsync(p => p.Reference == reference);
        if (product is null) return ServiceResult.Fail("Produit introuvable.");

        if (product.Quantite < quantity)
            return ServiceResult.Fail($"Stock insuffisant. Stock actuel: {product.Quantite}.");

        product.Quantite -= quantity;
        await _db.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}