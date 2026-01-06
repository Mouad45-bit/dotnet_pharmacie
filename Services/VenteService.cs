using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class VenteService : IVenteService
{
    private readonly PharmacieDbContext _db;

    public VenteService(PharmacieDbContext db) => _db = db;

    public async Task<ServiceResult<IVenteService.SaleCreatedInfo>> CreateAsync(
        string clientId,
        string productRef,
        int quantity,
        bool createInvoice)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return ServiceResult<IVenteService.SaleCreatedInfo>.Fail("Client requis.");

        if (string.IsNullOrWhiteSpace(productRef))
            return ServiceResult<IVenteService.SaleCreatedInfo>.Fail("Produit requis.");

        if (quantity <= 0)
            return ServiceResult<IVenteService.SaleCreatedInfo>.Fail("Quantité invalide.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            // ⚠️ Ici on veut des entités TRACKÉES (pas AsNoTracking),
            // car on modifie stock + points client
            var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == clientId);
            if (client is null)
                return ServiceResult<IVenteService.SaleCreatedInfo>.Fail("Client introuvable.");

            var product = await _db.Produits.FirstOrDefaultAsync(p => p.Reference == productRef);
            if (product is null)
                return ServiceResult<IVenteService.SaleCreatedInfo>.Fail("Produit introuvable.");

            if (product.Quantite < quantity)
                return ServiceResult<IVenteService.SaleCreatedInfo>.Fail(
                    $"Stock insuffisant pour '{product.Nom}'. Stock actuel: {product.Quantite}."
                );

            var prixUnitaire = product.Prix;
            var total = quantity * prixUnitaire;

            // 1) Vente
            var vente = new Vente
            {
                ClientId = client.Id,
                DateVente = DateTime.Now
            };

            // 2) Ligne
            vente.Lignes.Add(new VenteLigne
            {
                VenteId = vente.Id,
                ProduitReference = product.Reference,
                Quantite = quantity,
                PrixUnitaire = prixUnitaire
            });

            // 3) Stock
            product.Quantite -= quantity;

            // 4) Facture (option)
            if (createInvoice)
            {
                vente.Facture = new Facture
                {
                    VenteId = vente.Id,
                    Sujet = $"Facture vente {vente.Id}"
                };
            }

            // 5) Fidélité (simple) : 1 point / 10 DH
            var pointsAdded = (int)Math.Floor((double)(total / 10m));
            if (pointsAdded > 0)
                client.LoyaltyPoints += pointsAdded;

            // Statut client (même règle que ton code)
            client.Status = client.LoyaltyPoints >= 120 ? "Or"
                        : client.LoyaltyPoints >= 60 ? "Argent"
                        : "Nouveau";

            _db.Ventes.Add(vente);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            return ServiceResult<IVenteService.SaleCreatedInfo>.Ok(
                new IVenteService.SaleCreatedInfo(
                    SaleId: vente.Id,
                    Total: total,
                    ClientName: client.Name,
                    ProductName: product.Nom,
                    Quantity: quantity,
                    InvoiceCreated: createInvoice,
                    PointsAdded: pointsAdded
                )
            );
        }
        catch
        {
            await tx.RollbackAsync();
            return ServiceResult<IVenteService.SaleCreatedInfo>.Fail("Erreur lors de l’enregistrement de la vente.");
        }
    }
}