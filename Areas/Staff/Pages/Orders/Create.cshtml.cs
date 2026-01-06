using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Services;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly IFournisseurService _fournisseurs;
        private readonly IProduitService _produits;
        private readonly ICommandeService _commandeService;

        public CreateModel(
            IFournisseurService fournisseurs,
            IProduitService produits,
            ICommandeService commandeService)
        {
            _fournisseurs = fournisseurs;
            _produits = produits;
            _commandeService = commandeService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SupplierId { get; set; }

        [BindProperty]
        public CreateOrderForm Form { get; set; } = new();

        public string? ErrorMessage { get; private set; }

        public List<SupplierItem> Suppliers { get; private set; } = new();
        public List<ProductItem> Products { get; private set; } = new();

        public async Task OnGetAsync()
        {
            await LoadListsAsync();

            if (!string.IsNullOrWhiteSpace(SupplierId))
                Form.SupplierId = SupplierId!;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadListsAsync();

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez corriger les champs en erreur.";
                return Page();
            }

            var result = await _commandeService.CreateAsync(Form.SupplierId, Form.ProductId, Form.Quantity);

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "Impossible de créer la commande.";
                return Page();
            }

            var info = result.Data!;
            TempData["Toast.Success"] =
                $"Commande créée : {Form.Quantity} × {info.ProductName} chez {info.SupplierName} (Total: {info.Total:N2}).";

            return RedirectToPage("/Orders/History", new { area = "Staff" });
        }

        private async Task LoadListsAsync()
        {
            // Fournisseurs via service
            var supplierList = await _fournisseurs.GetListAsync(null); // renvoie SupplierListItem (DTO)
            Suppliers = supplierList
                .OrderByDescending(s => s.NoteGlobale) // adapte si ton DTO a NoteGlobale
                .ThenBy(s => s.Nom)
                .Select(s => new SupplierItem(s.Id, s.Nom, s.NoteGlobale))
                .ToList();

            // Produits via service
            var products = await _produits.GetAllAsync(); // adapte si méthode différente
            Products = products
                .OrderBy(p => p.Nom)
                .Select(p => new ProductItem(p.Reference, p.Nom, p.Quantite))
                .ToList();
        }

        public record SupplierItem(string Id, string Name, int Rating);
        public record ProductItem(string Id, string Name, int Stock);

        public class CreateOrderForm
        {
            [Required(ErrorMessage = "Fournisseur requis")]
            public string SupplierId { get; set; } = "";

            [Required(ErrorMessage = "Produit requis")]
            public string ProductId { get; set; } = "";

            [Range(1, 999999, ErrorMessage = "Quantité invalide")]
            public int Quantity { get; set; } = 1;

            [StringLength(200, ErrorMessage = "Note trop longue (max 200)")]
            public string? Note { get; set; }
        }
    }
}