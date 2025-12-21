using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace project_pharmacie.Areas.Staff.Pages.Products
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Query { get; set; }

        public List<ProductRow> Rows { get; private set; } = new();

        public void OnGet()
        {
            // MOCK — plus tard: service/DB
            var all = new List<ProductRow>
            {
                new(1, "Doliprane 1g", "Antalgique", 12, 10, 18.50m),
                new(2, "Vitamine C", "Compléments", 3, 8, 45.00m),
                new(3, "Aerius", "Allergie", 0, 5, 79.90m),
                new(4, "Smecta", "Digestif", 6, 6, 34.00m),
                new(5, "Biseptine", "Antiseptique", 25, 10, 32.00m),
            };

            if (!string.IsNullOrWhiteSpace(Query))
            {
                var q = Query.Trim().ToLowerInvariant();
                Rows = all.Where(p =>
                        p.Name.ToLowerInvariant().Contains(q) ||
                        p.Category.ToLowerInvariant().Contains(q)
                    )
                    .ToList();
            }
            else
            {
                Rows = all;
            }
        }

        public (string badgeCls, string text) GetStockBadge(int stock, int reorder)
        {
            if (stock == 0)
                return ("bg-red-50 text-red-700 ring-red-200", "Rupture");

            if (stock <= reorder)
                return ("bg-amber-50 text-amber-700 ring-amber-200", "Alerte");

            return ("bg-emerald-50 text-emerald-700 ring-emerald-200", "OK");
        }

        public record ProductRow(
            int Id,
            string Name,
            string Category,
            int Stock,
            int ReorderLevel,
            decimal UnitPrice
        );
    }
}
