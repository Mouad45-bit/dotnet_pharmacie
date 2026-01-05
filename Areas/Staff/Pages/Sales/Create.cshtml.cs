using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Sales;

public class CreateModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public CreateModel(PharmacieDbContext db) => _db = db;

    // Pour pré-sélectionner un client depuis History ("Refaire")
    [BindProperty(SupportsGet = true)]
    public string? CustomerHint { get; set; }

    [BindProperty]
    public SaleForm NewSale { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public List<ClientItem> Clients { get; private set; } = new();
    public List<ProductItem> Products { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadListsAsync();

        // Pré-sélection client via CustomerHint (nom)
        if (!string.IsNullOrWhiteSpace(CustomerHint))
        {
            var hint = CustomerHint.Trim();

            var client = await _db.Clients.AsNoTracking()
                .OrderByDescending(c => c.LoyaltyPoints)
                .FirstOrDefaultAsync(c => c.Name == hint);

            if (client is not null)
                NewSale.ClientId = client.Id;
        }

        // Par défaut : facture cochée
        NewSale.CreateInvoice = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadListsAsync();

        if (!ModelState.IsValid) return Page();

        // Sécurité : client existant
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == NewSale.ClientId);
        if (client is null)
        {
            ErrorMessage = "Client invalide.";
            return Page();
        }

        // Sécurité : produit existant (Produit.Reference)
        var product = await _db.Produits.FirstOrDefaultAsync(p => p.Reference == NewSale.ProductId);
        if (product is null)
        {
            ErrorMessage = "Produit invalide.";
            return Page();
        }

        if (NewSale.Quantity <= 0)
        {
            ErrorMessage = "Quantité invalide.";
            return Page();
        }

        // Vérif stock
        if (product.Quantite < NewSale.Quantity)
        {
            ErrorMessage = $"Stock insuffisant pour '{product.Nom}'. Stock actuel: {product.Quantite}.";
            return Page();
        }

        // Calcul serveur
        var prixUnitaire = product.Prix;
        var total = NewSale.Quantity * prixUnitaire;

        // Transaction (évite stock incohérent)
        await using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            // 1) Créer vente
            var vente = new Vente
            {
                ClientId = client.Id,
                DateVente = DateTime.Now
            };

            // 2) Ligne vente
            vente.Lignes.Add(new VenteLigne
            {
                VenteId = vente.Id,
                ProduitReference = product.Reference,
                Quantite = NewSale.Quantity,
                PrixUnitaire = prixUnitaire
            });

            // 3) Décrémenter stock
            product.Quantite -= NewSale.Quantity;

            // 4) (Option) Créer facture 1-1
            if (NewSale.CreateInvoice)
            {
                vente.Facture = new Facture
                {
                    VenteId = vente.Id,
                    Sujet = $"Facture vente {vente.Id}"
                };
            }

            // 5) (Option simple) points fidélité : 1 point par 10 DH
            var pointsAjoutes = (int)Math.Floor((double)(total / 10m));
            if (pointsAjoutes > 0)
                client.LoyaltyPoints += pointsAjoutes;

            // Recalcul statut client (même logique que Clients/Edit)
            client.Status = client.LoyaltyPoints >= 120 ? "Or"
                         : client.LoyaltyPoints >= 60 ? "Argent"
                         : "Nouveau";

            _db.Ventes.Add(vente);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();

            TempData["Toast.Success"] =
                $"Vente enregistrée : {NewSale.Quantity} × {product.Nom} pour {client.Name} (Total: {total:N2}).";

            return RedirectToPage("./History");
        }
        catch
        {
            await tx.RollbackAsync();
            ErrorMessage = "Erreur lors de l’enregistrement de la vente.";
            return Page();
        }
    }

    private async Task LoadListsAsync()
    {
        Clients = await _db.Clients
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ClientItem(c.Id, c.Name, c.LoyaltyPoints))
            .ToListAsync();

        Products = await _db.Produits
            .AsNoTracking()
            .OrderBy(p => p.Nom)
            .Select(p => new ProductItem(p.Reference, p.Nom, p.Quantite, p.Prix))
            .ToListAsync();
    }

    public record ClientItem(string Id, string Name, int Points);
    public record ProductItem(string Id, string Name, int Stock, decimal Prix);

    public class SaleForm
    {
        [Required(ErrorMessage = "Client requis")]
        public string ClientId { get; set; } = "";

        // ProductId = Produit.Reference
        [Required(ErrorMessage = "Produit requis")]
        public string ProductId { get; set; } = "";

        [Range(1, 9999, ErrorMessage = "Quantité invalide")]
        public int Quantity { get; set; } = 1;

        public bool CreateInvoice { get; set; } = true;
    }
}