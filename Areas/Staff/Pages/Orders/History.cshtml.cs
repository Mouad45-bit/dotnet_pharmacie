using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_pharmacie.Areas.Staff.Pages.Orders
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

        public List<OrderRow> Rows { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var query = _db.Commandes
                .AsNoTracking()
                .Include(c => c.Fournisseur)
                .Include(c => c.Lignes)
                    .ThenInclude(l => l.Produit)
                .AsQueryable();

            // Recherche fournisseur/produit
            if (!string.IsNullOrWhiteSpace(Query))
            {
                var term = Query.Trim();

                query = query.Where(c =>
                    (c.Fournisseur != null && EF.Functions.Like(c.Fournisseur.Nom, $"%{term}%"))
                    ||
                    c.Lignes.Any(l =>
                        EF.Functions.Like(l.ProduitReference, $"%{term}%")
                        || (l.Produit != null && EF.Functions.Like(l.Produit.Nom, $"%{term}%"))
                    )
                );
            }

            var list = await query
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            Rows = list.Select(c =>
            {
                var status = DeriveStatus(c.Date);

                var qty = c.Lignes.Sum(l => l.Quantite);

                var productLabel = "-";
                if (c.Lignes.Count == 1)
                {
                    var l = c.Lignes.First();
                    productLabel = l.Produit?.Nom ?? l.ProduitReference;
                }
                else if (c.Lignes.Count > 1)
                {
                    var first = c.Lignes.First();
                    var firstName = first.Produit?.Nom ?? first.ProduitReference;
                    productLabel = $"{firstName} (+{c.Lignes.Count - 1})";
                }

                return new OrderRow(
                    Id: c.Id,
                    SupplierId: c.FournisseurId,
                    SupplierName: c.Fournisseur?.Nom ?? "-",
                    ProductName: productLabel,
                    Quantity: qty,
                    Status: status,
                    CreatedAt: c.Date,
                    Total: c.PrixTotal
                );
            }).ToList();

            // Filtre statut (statut dérivé car pas de colonne Status en DB)
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                Rows = Rows
                    .Where(r => r.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        // Statut “simulé” sans changer le schéma :
        // - < 36h : PENDING
        // - sinon : DELIVERED
        private static string DeriveStatus(DateTime date)
        {
            var age = DateTime.Now - date;
            return age.TotalHours < 36 ? "PENDING" : "DELIVERED";
        }

        public (string cls, string label) GetStatusBadge(string status)
        {
            return (status ?? "").ToUpperInvariant() switch
            {
                "DELIVERED" => ("bg-emerald-50 text-emerald-700 ring-emerald-200", "Livrée"),
                _ => ("bg-amber-50 text-amber-700 ring-amber-200", "En attente"),
            };
        }

        public record OrderRow(
            string Id,
            string SupplierId,
            string SupplierName,
            string ProductName,
            int Quantity,
            string Status,
            DateTime CreatedAt,
            decimal Total
        );
    }
}