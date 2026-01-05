using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Services;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly PharmacieDbContext _db;
        private readonly ICommandeService _commandeService;

        public CreateModel(PharmacieDbContext db, ICommandeService commandeService)
        {
            _db = db;
            _commandeService = commandeService;
        }

        // Pour pré-sélectionner un fournisseur depuis /Suppliers (supplierId=...)
        // Maintenant string (car Fournisseur.Id est string)
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

            // Pré-sélection fournisseur si query string fournie
            if (!string.IsNullOrWhiteSpace(SupplierId))
            {
                Form.SupplierId = SupplierId!;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadListsAsync();

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez corriger les champs en erreur.";
                return Page();
            }

            // ✅ Toute la logique DB est dans le service
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
            Suppliers = await _db.Fournisseurs
                .AsNoTracking()
                .OrderByDescending(s => s.NoteGlobale)
                .ThenBy(s => s.Nom)
                .Select(s => new SupplierItem(s.Id, s.Nom, s.NoteGlobale))
                .ToListAsync();

            Products = await _db.Produits
                .AsNoTracking()
                .OrderBy(p => p.Nom)
                .Select(p => new ProductItem(p.Reference, p.Nom, p.Quantite))
                .ToListAsync();
        }

        public record SupplierItem(string Id, string Name, double Rating);
        public record ProductItem(string Id, string Name, int Stock);

        public class CreateOrderForm
        {
            [Required(ErrorMessage = "Fournisseur requis")]
            public string SupplierId { get; set; } = "";

            // ProductId = Produit.Reference (clé primaire string)
            [Required(ErrorMessage = "Produit requis")]
            public string ProductId { get; set; } = "";

            [Range(1, 999999, ErrorMessage = "Quantité invalide")]
            public int Quantity { get; set; } = 1;

            // On garde le champ pour l'UI mais on ne le stocke pas ici
            [StringLength(200, ErrorMessage = "Note trop longue (max 200)")]
            public string? Note { get; set; }
        }
    }
}