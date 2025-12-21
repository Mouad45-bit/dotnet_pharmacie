using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace project_pharmacie.Areas.Staff.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public int KpiTotalProducts { get; private set; }
        public int KpiLowStockCount { get; private set; }
        public int KpiTotalUnits { get; private set; }
        public decimal KpiStockValueMad { get; private set; }

        public List<ProductRow> LowStockProducts { get; private set; } = new();

        public void OnGet()
        {
            var products = new List<ProductRow>
            {
                new("Doliprane 1g", "Antalgique", 12, 10, 18.50m, "MedicaPlus"),
                new("Vitamine C", "Compléments", 3, 8, 45.00m, "BioSup"),
                new("Aerius", "Allergie", 0, 5, 79.90m, "PharmaDist"),
                new("Smecta", "Digestif", 6, 6, 34.00m, "MedicaPlus"),
                new("Biseptine", "Antiseptique", 25, 10, 32.00m, "PharmaDist"),
            };

            KpiTotalProducts = products.Count;
            KpiTotalUnits = products.Sum(p => p.Stock);
            KpiStockValueMad = products.Sum(p => p.Stock * p.UnitPrice);

            LowStockProducts = products
                .Where(p => p.Stock <= p.ReorderLevel)
                .OrderBy(p => p.Stock)
                .ToList();

            KpiLowStockCount = LowStockProducts.Count;
        }

        public record ProductRow(
            string Name,
            string Category,
            int Stock,
            int ReorderLevel,
            decimal UnitPrice,
            string SupplierHint
        );
    }
}
