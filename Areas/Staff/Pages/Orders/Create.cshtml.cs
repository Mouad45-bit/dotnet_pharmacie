using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly PharmacieDbContext _db;

        public CreateModel(PharmacieDbContext db) => _db = db;

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

            // Vérifier que le fournisseur existe en base
            var supplier = await _db.Fournisseurs
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == Form.SupplierId);

            // Vérifier que le produit existe en base (ProductId = Produit.Reference)
            var product = await _db.Produits
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Reference == Form.ProductId);

            if (supplier is null || product is null)
            {
                ErrorMessage = "Fournisseur ou produit invalide (introuvable en base).";
                return Page();
            }

            // Calcul serveur (important)
            var prixUnitaire = product.Prix;
            var prixTotal = Form.Quantity * prixUnitaire;

            // Créer la commande
            var commande = new Commande
            {
                FournisseurId = supplier.Id,
                Date = DateTime.Now,
                PrixTotal = prixTotal,
                Note = 0,
                PersonnelId = null
            };

            // Ajouter une ligne (1ère version : 1 seul produit)
            commande.Lignes.Add(new CommandeLigne
            {
                CommandeId = commande.Id,
                ProduitReference = product.Reference,
                Quantite = Form.Quantity,
                PrixUnitaire = prixUnitaire
            });

            _db.Commandes.Add(commande);
            await _db.SaveChangesAsync();

            TempData["Toast.Success"] =
                $"Commande créée : {Form.Quantity} × {product.Nom} chez {supplier.Nom} (Total: {prixTotal:N2}).";

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

            // On garde le champ pour l'UI mais on ne le stocke pas (pas de colonne comment)
            [StringLength(200, ErrorMessage = "Note trop longue (max 200)")]
            public string? Note { get; set; }
        }
    }
}