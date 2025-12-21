using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Products
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public CreateProductForm Form { get; set; } = new();

        public string? ErrorMessage { get; private set; }

        public void OnGet()
        {
            // valeurs par défaut (optionnel)
            Form.Stock = 0;
            Form.ReorderLevel = 5;
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez corriger les champs en erreur.";
                return Page();
            }

            // ? MOCK : ici on branchera plus tard sur un service/DB
            // Exemple futur : _productService.Create(Form)

            TempData["Toast.Success"] = $"Produit '{Form.Name}' ajouté (mock).";
            return RedirectToPage("/Products/Index", new { area = "Staff" });
        }

        public class CreateProductForm
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
