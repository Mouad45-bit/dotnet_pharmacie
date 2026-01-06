using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Services;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Sales;

public class CreateModel : PageModel
{
    private readonly PharmacieDbContext _db;
    private readonly IVenteService _venteService;

    public CreateModel(PharmacieDbContext db, IVenteService venteService)
    {
        _db = db;
        _venteService = venteService;
    }

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

        // Pré-sélection client via CustomerHint (nom exact)
        if (!string.IsNullOrWhiteSpace(CustomerHint))
        {
            var hint = CustomerHint.Trim();

            var client = await _db.Clients.AsNoTracking()
                .OrderBy(c => c.Name)
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

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Veuillez corriger les champs en erreur.";
            return Page();
        }

        var result = await _venteService.CreateAsync(
            clientId: NewSale.ClientId,
            productRef: NewSale.ProductId,
            quantity: NewSale.Quantity,
            createInvoice: NewSale.CreateInvoice
        );

        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Impossible d’enregistrer la vente.";
            return Page();
        }

        var info = result.Data!;
        TempData["Toast.Success"] =
            $"Vente enregistrée : {info.Quantity} × {info.ProductName} pour {info.ClientName} " +
            $"(Total: {info.Total:N2})" +
            (info.InvoiceCreated ? " — Facture créée." : ".");

        return RedirectToPage("./History");
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