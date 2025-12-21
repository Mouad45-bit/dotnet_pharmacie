using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace project_pharmacie.Areas.Staff.Pages.Suppliers
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Sort { get; set; } = "rating_desc";

        public List<SupplierRow> Rows { get; private set; } = new();

        // Summary
        public int TotalSuppliers { get; private set; }
        public double AverageRating { get; private set; }
        public int TotalRatingsCount { get; private set; }
        public string BestSupplierName { get; private set; } = "-";
        public double BestSupplierRating { get; private set; }

        public void OnGet()
        {
            var all = MockSuppliers();

            TotalSuppliers = all.Count;
            TotalRatingsCount = all.Sum(x => x.RatingsCount);

            AverageRating = all.Count == 0 ? 0 : all.Average(x => x.Rating);

            var best = all.OrderByDescending(x => x.Rating).ThenByDescending(x => x.RatingsCount).FirstOrDefault();
            if (best is not null)
            {
                BestSupplierName = best.Name;
                BestSupplierRating = best.Rating;
            }

            Rows = Sort?.ToLowerInvariant() switch
            {
                "rating_asc" => all.OrderBy(x => x.Rating).ThenByDescending(x => x.RatingsCount).ToList(),
                "name" => all.OrderBy(x => x.Name).ToList(),
                _ => all.OrderByDescending(x => x.Rating).ThenByDescending(x => x.RatingsCount).ToList(), // rating_desc default
            };
        }

        public (string badgeCls, string text) GetRatingBadge(double rating)
        {
            if (rating >= 4.5)
                return ("bg-emerald-50 text-emerald-700 ring-emerald-200", "Excellent");

            if (rating >= 3.5)
                return ("bg-amber-50 text-amber-700 ring-amber-200", "Bon");

            return ("bg-red-50 text-red-700 ring-red-200", "À surveiller");
        }

        private List<SupplierRow> MockSuppliers()
        {
            // MOCK — plus tard: _supplierService.List()
            return new List<SupplierRow>
            {
                new(1, "MedicaPlus", "+212 6 12 34 56 78", 4.7, 123, true),
                new(2, "PharmaDist", "+212 6 98 76 54 32", 4.3, 89, false),
                new(3, "BioSup", "+212 6 11 22 33 44", 3.9, 41, false),
                new(4, "FastSupply", "+212 6 55 66 77 88", 3.2, 19, false),
            };
        }

        public record SupplierRow(
            int Id,
            string Name,
            string Phone,
            double Rating,
            int RatingsCount,
            bool IsPreferred
        );
    }
}
