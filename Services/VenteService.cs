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
            // TRACKÉ (pas AsNoTracking) car on modifie stock + points
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

            var vente = new Vente
            {
                ClientId = client.Id,
                DateVente = DateTime.Now
            };

            vente.Lignes.Add(new VenteLigne
            {
                VenteId = vente.Id,
                ProduitReference = product.Reference,
                Quantite = quantity,
                PrixUnitaire = prixUnitaire
            });

            // Stock
            product.Quantite -= quantity;

            // Facture (option)
            if (createInvoice)
            {
                vente.Facture = new Facture
                {
                    VenteId = vente.Id,
                    Sujet = $"Facture vente {vente.Id}"
                };
            }

            // Fidélité : 1 point / 10 DH
            var pointsAdded = (int)Math.Floor((double)(total / 10m));
            if (pointsAdded > 0)
                client.LoyaltyPoints += pointsAdded;

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

    public async Task<ServiceResult<List<IVenteService.SaleHistoryItem>>> ListAsync(string? query = null)
    {
        var q = _db.Ventes
            .AsNoTracking()
            .Include(v => v.Client)
            .Include(v => v.Lignes)
                .ThenInclude(l => l.Produit)
            .Include(v => v.Facture)
            .AsQueryable();

        // Recherche client/produit
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();

            q = q.Where(v =>
                (v.Client != null && EF.Functions.Like(v.Client.Name, $"%{term}%"))
                ||
                v.Lignes.Any(l =>
                    EF.Functions.Like(l.ProduitReference, $"%{term}%")
                    || (l.Produit != null && EF.Functions.Like(l.Produit.Nom, $"%{term}%"))
                )
            );
        }

        var list = await q
            .OrderByDescending(v => v.DateVente)
            .ToListAsync();

        var rows = list.Select(v =>
        {
            var status = v.Facture != null ? "PAID" : "PENDING";

            var qty = v.Lignes.Sum(l => l.Quantite);
            var total = v.Lignes.Sum(l => l.Quantite * l.PrixUnitaire);

            var productLabel = "-";
            if (v.Lignes.Count == 1)
            {
                var l = v.Lignes.First();
                productLabel = l.Produit?.Nom ?? l.ProduitReference;
            }
            else if (v.Lignes.Count > 1)
            {
                var first = v.Lignes.First();
                var firstName = first.Produit?.Nom ?? first.ProduitReference;
                productLabel = $"{firstName} (+{v.Lignes.Count - 1})";
            }

            return new IVenteService.SaleHistoryItem(
                Id: v.Id,
                CustomerName: v.Client?.Name ?? "-",
                DrugName: productLabel,
                Quantity: qty,
                TotalPrice: total,
                Status: status,
                Date: v.DateVente
            );
        }).ToList();

        return ServiceResult<List<IVenteService.SaleHistoryItem>>.Ok(rows);
    }
}