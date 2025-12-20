using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Products
{
    public class EditModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public EditProductForm Form { get; set; } = new();

        public bool Found { get; private set; }
        public string? ErrorMessage { get; private set; }

        public void OnGet()
        {
            // MOCK — plus tard: _productService.GetById(Id)
            var p = MockFindById(Id);
            if (p is null)
            {
                Found = false;
                return;
            }

            Found = true;
            Form = new EditProductForm
            {
                Name = p.Name,
                Category = p.Category,
                UnitPrice = p.UnitPrice,
                Stock = p.Stock,
                ReorderLevel = p.ReorderLevel
            };
        }

        public IActionResult OnPost()
        {
            // Recharger "Found" pour afficher la page proprement en cas d’erreur
            var existing = MockFindById(Id);
            Found = existing is not null;

            if (!Found)
            {
                ErrorMessage = "Produit introuvable.";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez corriger les champs en erreur.";
                return Page();
            }

            // ✅ MOCK : ici on branchera la mise à jour DB/service
            // Exemple futur: _productService.Update(Id, Form)

            TempData["Toast.Success"] = $"Produit '{Form.Name}' modifié (mock).";
            return RedirectToPage("/Products/Index", new { area = "Staff" });
        }

        private Product? MockFindById(int id)
        {
            // Même base mock que Products/Index (pour cohérence)
            return id switch
            {
                1 => new Product("Doliprane 1g", "Antalgique", 18.50m, 12, 10),
                2 => new Product("Vitamine C", "Compléments", 45.00m, 3, 8),
                3 => new Product("Aerius", "Allergie", 79.90m, 0, 5),
                4 => new Product("Smecta", "Digestif", 34.00m, 6, 6),
                5 => new Product("Biseptine", "Antiseptique", 32.00m, 25, 10),
                _ => null
            };
        }

        private record Product(string Name, string Category, decimal UnitPrice, int Stock, int ReorderLevel);

        public class EditProductForm
        {
            [Required(ErrorMessage = "Nom requis")]
            [StringLength(80, ErrorMessage = "Max 80 caractères")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "Catégorie requise")]
            [StringLength(50, ErrorMessage = "Max 50 caractères")]
            public string Category { get; set; } = string.Empty;

            [Range(0.01, 999999, ErrorMessage = "Prix invalide")]
            public decimal UnitPrice { get; set; }

            [Range(0, 999999, ErrorMessage = "Stock invalide")]
            public int Stock { get; set; }

            [Range(0, 999999, ErrorMessage = "Seuil invalide")]
            public int ReorderLevel { get; set; }
        }
    }
}
