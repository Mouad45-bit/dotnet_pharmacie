using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace project_pharmacie.Areas.Staff.Pages.Suppliers
{
    public class RateModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int SupplierId { get; set; }

        [BindProperty]
        public RateForm Form { get; set; } = new();

        public bool Found { get; private set; }
        public string SupplierName { get; private set; } = "-";
        public double CurrentRating { get; private set; }
        public int CurrentRatingsCount { get; private set; }

        public string? ErrorMessage { get; private set; }

        public void OnGet()
        {
            LoadSupplierOrNotFound();
        }

        public IActionResult OnPost()
        {
            LoadSupplierOrNotFound();
            if (!Found)
            {
                ErrorMessage = "Fournisseur introuvable.";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez corriger les champs en erreur.";
                return Page();
            }

            // ✅ MOCK: plus tard -> _supplierService.AddRating(SupplierId, Form.Rating, Form.Comment)
            TempData["Toast.Success"] = $"Note envoyée (mock) : {Form.Rating}/5 pour {SupplierName}.";
            return RedirectToPage("/Suppliers/Index", new { area = "Staff" });
        }

        private void LoadSupplierOrNotFound()
        {
            var s = MockSuppliers().FirstOrDefault(x => x.Id == SupplierId);
            if (s is null)
            {
                Found = false;
                return;
            }

            Found = true;
            SupplierName = s.Name;
            CurrentRating = s.Rating;
            CurrentRatingsCount = s.RatingsCount;

            if (Form.Rating == 0)
                Form.Rating = 5; // default sympa
        }

        private List<SupplierRow> MockSuppliers() => new()
        {
            new(1, "MedicaPlus", 4.7, 123),
            new(2, "PharmaDist", 4.3, 89),
            new(3, "BioSup", 3.9, 41),
            new(4, "FastSupply", 3.2, 19),
        };

        private record SupplierRow(int Id, string Name, double Rating, int RatingsCount);

        public class RateForm
        {
            [Range(1, 5, ErrorMessage = "La note doit être entre 1 et 5")]
            public int Rating { get; set; }

            [StringLength(200, ErrorMessage = "Commentaire trop long (max 200)")]
            public string? Comment { get; set; }
        }
    }
}
