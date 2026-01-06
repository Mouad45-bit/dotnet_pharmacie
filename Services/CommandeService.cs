using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class CommandeService : ICommandeService
{
    private readonly PharmacieDbContext _db;

    public CommandeService(PharmacieDbContext db) => _db = db;

    public async Task<ServiceResult<ICommandeService.OrderCreatedInfo>> CreateAsync(
        string supplierId,
        string productRef,
        int quantity)
    {
        if (string.IsNullOrWhiteSpace(supplierId))
            return ServiceResult<ICommandeService.OrderCreatedInfo>.Fail("Fournisseur requis.");

        if (string.IsNullOrWhiteSpace(productRef))
            return ServiceResult<ICommandeService.OrderCreatedInfo>.Fail("Produit requis.");

        if (quantity <= 0)
            return ServiceResult<ICommandeService.OrderCreatedInfo>.Fail("Quantité invalide.");

        var supplier = await _db.Fournisseurs
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == supplierId);

        if (supplier is null)
            return ServiceResult<ICommandeService.OrderCreatedInfo>.Fail("Fournisseur introuvable.");

        var product = await _db.Produits
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Reference == productRef);

        if (product is null)
            return ServiceResult<ICommandeService.OrderCreatedInfo>.Fail("Produit introuvable.");

        var prixUnitaire = product.Prix;
        var prixTotal = quantity * prixUnitaire;

        var commande = new Commande
        {
            FournisseurId = supplier.Id,
            Date = DateTime.Now,
            PrixTotal = prixTotal,
            Note = 0,
            PersonnelId = null
        };

        commande.Lignes.Add(new CommandeLigne
        {
            CommandeId = commande.Id,
            ProduitReference = product.Reference,
            Quantite = quantity,
            PrixUnitaire = prixUnitaire
        });

        _db.Commandes.Add(commande);
        await _db.SaveChangesAsync();

        return ServiceResult<ICommandeService.OrderCreatedInfo>.Ok(
            new ICommandeService.OrderCreatedInfo(
                OrderId: commande.Id,
                Total: prixTotal,
                SupplierName: supplier.Nom,
                ProductName: product.Nom
            )
        );
    }

    public async Task<ServiceResult<List<ICommandeService.OrderHistoryItem>>> ListAsync(string? query = null)
    {
        var q = _db.Commandes
            .AsNoTracking()
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes)
                .ThenInclude(l => l.Produit)
            .AsQueryable();

        // Recherche fournisseur/produit
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();

            q = q.Where(c =>
                (c.Fournisseur != null && EF.Functions.Like(c.Fournisseur.Nom, $"%{term}%"))
                ||
                c.Lignes.Any(l =>
                    EF.Functions.Like(l.ProduitReference, $"%{term}%")
                    || (l.Produit != null && EF.Functions.Like(l.Produit.Nom, $"%{term}%"))
                )
            );
        }

        var list = await q
            .OrderByDescending(c => c.Date)
            .ToListAsync();

        var rows = list.Select(c =>
        {
            var qty = c.Lignes.Sum(l => l.Quantite);

            var productLabel = "-";
            if (c.Lignes.Count == 1)
            {
                var l = c.Lignes.First();
                productLabel = l.Produit?.Nom ?? l.ProduitReference;
            }
            else if (c.Lignes.Count > 1)
            {
                var first = c.Lignes.First();
                var firstName = first.Produit?.Nom ?? first.ProduitReference;
                productLabel = $"{firstName} (+{c.Lignes.Count - 1})";
            }

            return new ICommandeService.OrderHistoryItem(
                Id: c.Id,
                SupplierId: c.FournisseurId,
                SupplierName: c.Fournisseur?.Nom ?? "-",
                ProductName: productLabel,
                Quantity: qty,
                CreatedAt: c.Date,
                Total: c.PrixTotal
            );
        }).ToList();

        return ServiceResult<List<ICommandeService.OrderHistoryItem>>.Ok(rows);
    }
}
