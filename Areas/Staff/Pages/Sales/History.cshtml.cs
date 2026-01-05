using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_pharmacie.Areas.Staff.Pages.Sales
{
    public class HistoryModel : PageModel
    {
        private readonly PharmacieDbContext _db;

        public HistoryModel(PharmacieDbContext db) => _db = db;

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public string StatusFilter => (Status ?? "").Trim().ToUpperInvariant();
        public string Query => Q ?? "";

        public List<SaleRow> Sales { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var query = _db.Ventes
                .AsNoTracking()
                .Include(v => v.Client)
                .Include(v => v.Lignes)
                    .ThenInclude(l => l.Produit)
                .Include(v => v.Facture)
                .AsQueryable();

            // Recherche client/produit
            if (!string.IsNullOrWhiteSpace(Query))
            {
                var term = Query.Trim();

                query = query.Where(v =>
                    (v.Client != null && EF.Functions.Like(v.Client.Name, $"%{term}%"))
                    ||
                    v.Lignes.Any(l =>
                        EF.Functions.Like(l.ProduitReference, $"%{term}%")
                        || (l.Produit != null && EF.Functions.Like(l.Produit.Nom, $"%{term}%"))
                    )
                );
            }

            var list = await query
                .OrderByDescending(v => v.DateVente)
                .ToListAsync();

            Sales = list.Select(v =>
            {
                var status = v.Facture != null ? "PAID" : "PENDING";

                var qty = v.Lignes.Sum(l => l.Quantite);
                var total = v.Lignes.Sum(l => l.Quantite * l.PrixUnitaire);

                var productLabel = "-";
                if (v.Lignes.Count == 1)
                {
                    var l = v.Lignes.First();
                    productLabel = l.Produit?.Nom ?? l.ProduitReference;
                }
                else if (v.Lignes.Count > 1)
                {
                    var first = v.Lignes.First();
                    var firstName = first.Produit?.Nom ?? first.ProduitReference;
                    productLabel = $"{firstName} (+{v.Lignes.Count - 1})";
                }

                return new SaleRow(
                    Id: v.Id,
                    CustomerName: v.Client?.Name ?? "-",
                    DrugName: productLabel,
                    Quantity: qty,
                    TotalPrice: total,
                    Status: status,
                    Date: v.DateVente
                );
            }).ToList();

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                Sales = Sales
                    .Where(s => s.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        public (string cls, string label) GetStatusBadge(string status)
        {
            return (status ?? "").ToUpperInvariant() switch
            {
                "PAID" => ("bg-emerald-50 text-emerald-700 ring-emerald-200", "Payée"),
                _ => ("bg-amber-50 text-amber-700 ring-amber-200", "En attente"),
            };
        }

        public record SaleRow(
            string Id,
            string CustomerName,
            string DrugName,
            int Quantity,
            decimal TotalPrice,
            string Status,
            DateTime Date
        );
    }
}