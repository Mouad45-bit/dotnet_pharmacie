using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class CommandeService : ICommandeService
{
    private readonly PharmacieDbContext _db;

    public CommandeService(PharmacieDbContext db) => _db = db;

    public async Task<ServiceResult<ICommandeService.OrderCreatedInfo>> CreateAsync(string supplierId, string productRef, int quantity)
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
}