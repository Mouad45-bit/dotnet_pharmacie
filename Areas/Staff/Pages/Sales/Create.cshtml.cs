using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Services;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Sales;

public class CreateModel : PageModel
{
    private readonly IClientService _clients;
    private readonly IProduitService _produits;
    private readonly IVenteService _venteService;

    public CreateModel(IClientService clients, IProduitService produits, IVenteService venteService)
    {
        _clients = clients;
        _produits = produits;
        _venteService = venteService;
    }

    // Pré-sélection propre depuis History (Refaire) : on passe l'ID
    [BindProperty(SupportsGet = true)]
    public string? ClientId { get; set; }

    [BindProperty]
    public SaleForm NewSale { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public List<ClientItem> Clients { get; private set; } = new();
    public List<ProductItem> Products { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadListsAsync();

        if (!string.IsNullOrWhiteSpace(ClientId))
            NewSale.ClientId = ClientId.Trim();

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
        // Clients (via service)
        var clients = await _clients.SearchAsync(null);
        Clients = clients
            .OrderBy(c => c.Nom) // adapte: Nom / Name
            .Select(c => new ClientItem(c.Id, c.Nom, c.PointsFidelite)) // adapte: PointsFidelite/LoyaltyPoints
            .ToList();

        // Produits (via service)
        var produits = await _produits.GetAllAsync(); // si ta méthode s'appelle différemment, adapte
        Products = produits
            .OrderBy(p => p.Nom)
            .Select(p => new ProductItem(p.Reference, p.Nom, p.Quantite, p.Prix))
            .ToList();
    }

    public record ClientItem(string Id, string Name, int Points);
    public record ProductItem(string Id, string Name, int Stock, decimal Prix);

    public class SaleForm
    {
        [Required(ErrorMessage = "Client requis")]
        public string ClientId { get; set; } = "";

        [Required(ErrorMessage = "Produit requis")]
        public string ProductId { get; set; } = "";

        [Range(1, 9999, ErrorMessage = "Quantité invalide")]
        public int Quantity { get; set; } = 1;

        public bool CreateInvoice { get; set; } = true;
    }
}
