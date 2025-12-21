using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace project_pharmacie.Areas.Staff.Pages.Orders
{
    public class CreateModel : PageModel
    {
        // Pour pré-sélectionner un fournisseur depuis /Suppliers (supplierId=...)
        [BindProperty(SupportsGet = true)]
        public int? SupplierId { get; set; }

        [BindProperty]
        public CreateOrderForm Form { get; set; } = new();

        public string? ErrorMessage { get; private set; }

        public List<SupplierItem> Suppliers { get; private set; } = new();
        public List<ProductItem> Products { get; private set; } = new();

        public void OnGet()
        {
            LoadLists();

            // Pré-sélection fournisseur si query string fournie
            if (SupplierId.HasValue && SupplierId.Value > 0)
            {
                Form.SupplierId = SupplierId.Value;
            }
        }

        public IActionResult OnPost()
        {
            LoadLists();

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez corriger les champs en erreur.";
                return Page();
            }

            var supplier = Suppliers.FirstOrDefault(s => s.Id == Form.SupplierId);
            var product = Products.FirstOrDefault(p => p.Id == Form.ProductId);

            if (supplier is null || product is null)
            {
                ErrorMessage = "Fournisseur ou produit invalide.";
                return Page();
            }

            // ✅ MOCK: plus tard -> _orderService.Create(...)
            TempData["Toast.Success"] =
                $"Commande créée (mock) : {Form.Quantity} × {product.Name} chez {supplier.Name}.";

            return RedirectToPage("/Orders/History", new { area = "Staff" });
        }

        private void LoadLists()
        {
            Suppliers = MockSuppliers()
                .OrderByDescending(s => s.Rating) // tri par note (desc)
                .ThenBy(s => s.Name)
                .ToList();

            Products = MockProducts()
                .OrderBy(p => p.Name)
                .ToList();
        }

        private List<SupplierItem> MockSuppliers() => new()
        {
            new(1, "MedicaPlus", 4.7),
            new(2, "PharmaDist", 4.3),
            new(3, "BioSup", 3.9),
            new(4, "FastSupply", 3.2),
        };

        private List<ProductItem> MockProducts() => new()
        {
            new(1, "Doliprane 1g", 12),
            new(2, "Vitamine C", 3),
            new(3, "Aerius", 0),
            new(4, "Smecta", 6),
            new(5, "Biseptine", 25),
        };

        public record SupplierItem(int Id, string Name, double Rating);
        public record ProductItem(int Id, string Name, int Stock);

        public class CreateOrderForm
        {
            [Required(ErrorMessage = "Fournisseur requis")]
            public int SupplierId { get; set; }

            [Required(ErrorMessage = "Produit requis")]
            public int ProductId { get; set; }

            [Range(1, 999999, ErrorMessage = "Quantité invalide")]
            public int Quantity { get; set; } = 1;

            [StringLength(200, ErrorMessage = "Note trop longue (max 200)")]
            public string? Note { get; set; }
        }
    }
}
