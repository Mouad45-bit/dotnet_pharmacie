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

        return await q
            .OrderBy(p => p.Nom)
            .ToListAsync();
    }

    public Task<Produit?> GetByReferenceAsync(string reference)
        => _db.Produits.FirstOrDefaultAsync(p => p.Reference == reference);

    public async Task<ServiceResult> CreateAsync(Produit produit)
    {
        if (produit is null) return ServiceResult.Fail("Produit invalide.");
        if (string.IsNullOrWhiteSpace(produit.Reference)) return ServiceResult.Fail("Référence requise.");
        if (string.IsNullOrWhiteSpace(produit.Nom)) return ServiceResult.Fail("Nom requis.");
        if (produit.Prix < 0) return ServiceResult.Fail("Prix invalide.");
        if (produit.Quantite < 0) return ServiceResult.Fail("Quantité invalide.");

        var exists = await _db.Produits.AnyAsync(p => p.Reference == produit.Reference);
        if (exists) return ServiceResult.Fail("Cette référence existe déjà.");

        _db.Produits.Add(produit);
        await _db.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateAsync(Produit produit)
    {
        if (produit is null) return ServiceResult.Fail("Produit invalide.");
        if (string.IsNullOrWhiteSpace(produit.Reference)) return ServiceResult.Fail("Référence requise.");
        if (string.IsNullOrWhiteSpace(produit.Nom)) return ServiceResult.Fail("Nom requis.");
        if (produit.Prix < 0) return ServiceResult.Fail("Prix invalide.");
        if (produit.Quantite < 0) return ServiceResult.Fail("Quantité invalide.");

        var existing = await _db.Produits.FirstOrDefaultAsync(p => p.Reference == produit.Reference);
        if (existing is null) return ServiceResult.Fail("Produit introuvable.");

        // Mise à jour des champs (référence = PK, on ne la change pas)
        existing.Nom = produit.Nom;
        existing.Prix = produit.Prix;
        existing.Quantite = produit.Quantite;
        existing.DatePeremption = produit.DatePeremption;

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference)) return ServiceResult.Fail("Référence requise.");

        var existing = await _db.Produits.FirstOrDefaultAsync(p => p.Reference == reference);
        if (existing is null) return ServiceResult.Fail("Produit introuvable.");

        _db.Produits.Remove(existing);
        await _db.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DecreaseStockAsync(string reference, int quantity)
    {
        if (string.IsNullOrWhiteSpace(reference)) return ServiceResult.Fail("Référence requise.");
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