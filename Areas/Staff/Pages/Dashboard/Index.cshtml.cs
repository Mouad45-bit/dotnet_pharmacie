using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;

namespace project_pharmacie.Areas.Staff.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly PharmacieDbContext _db;

        public IndexModel(PharmacieDbContext db) => _db = db;

        // Seuil simple (si tu n’as pas encore un champ ReorderLevel/Seuil en DB)
        public const int LowStockThreshold = 5;

        public int KpiTotalProducts { get; private set; }
        public int KpiLowStockCount { get; private set; }
        public int KpiTotalUnits { get; private set; }
        public decimal KpiStockValueMad { get; private set; }

        public List<ProductRow> LowStockProducts { get; private set; } = new();

        public async Task OnGetAsync()
        {
            // KPIs réels depuis la DB
            KpiTotalProducts = await _db.Produits
                .AsNoTracking()
                .CountAsync();

            KpiTotalUnits = await _db.Produits
                .AsNoTracking()
                .SumAsync(p => (int?)p.Quantite) ?? 0;

            KpiStockValueMad = await _db.Produits
                .AsNoTracking()
                .SumAsync(p => (decimal?)p.Quantite * p.Prix) ?? 0m;

            KpiLowStockCount = await _db.Produits
                .AsNoTracking()
                .CountAsync(p => p.Quantite <= LowStockThreshold);

            // Produits en alerte (stock <= seuil)
            LowStockProducts = await _db.Produits
                .AsNoTracking()
                .Where(p => p.Quantite <= LowStockThreshold)
                .OrderBy(p => p.Quantite)
                .Select(p => new ProductRow(
                    p.Nom,
                    p.Quantite,
                    LowStockThreshold,
                    p.Prix,
                    p.Fournisseurs
                        .Select(fp => fp.Fournisseur != null ? fp.Fournisseur.Nom : null)
                        .FirstOrDefault() ?? "—"
                ))
                .ToListAsync();
        }

        public record ProductRow(
            string Name,
            int Stock,
            int ReorderLevel,
            decimal UnitPrice,
            string SupplierHint
        );
    }
}