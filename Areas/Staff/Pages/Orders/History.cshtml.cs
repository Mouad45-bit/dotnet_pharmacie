using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace project_pharmacie.Areas.Staff.Pages.Orders
{
    public class HistoryModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public string StatusFilter => (Status ?? "").Trim().ToUpperInvariant();
        public string Query => Q ?? "";

        public List<OrderRow> Rows { get; private set; } = new();

        public void OnGet()
        {
            var all = MockOrders();

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                all = all
                    .Where(o => o.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(Query))
            {
                var q = Query.Trim().ToLowerInvariant();
                all = all
                    .Where(o => o.SupplierName.ToLowerInvariant().Contains(q)
                             || o.ProductName.ToLowerInvariant().Contains(q))
                    .ToList();
            }

            Rows = all
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
        }

        public (string cls, string label) GetStatusBadge(string status)
        {
            return (status ?? "").ToUpperInvariant() switch
            {
                "DELIVERED" => ("bg-emerald-50 text-emerald-700 ring-emerald-200", "Livrée"),
                "CANCELLED" => ("bg-red-50 text-red-700 ring-red-200", "Annulée"),
                _ => ("bg-amber-50 text-amber-700 ring-amber-200", "En attente"),
            };
        }

        private List<OrderRow> MockOrders()
        {
            var now = DateTime.Now;

            return new List<OrderRow>
            {
                new OrderRow(101, 1, "MedicaPlus", "Doliprane 1g", 10, "DELIVERED", now.AddDays(-2).AddHours(-3)),
                new OrderRow(102, 2, "PharmaDist", "Aerius", 20, "PENDING",   now.AddDays(-1).AddHours(-5)),
                new OrderRow(103, 3, "BioSup",     "Vitamine C", 15, "CANCELLED", now.AddDays(-7).AddHours(-2)),
                new OrderRow(104, 1, "MedicaPlus", "Smecta",     30, "DELIVERED", now.AddDays(-10).AddHours(-1)),
            };
        }

        public record OrderRow(
            int Id,
            int SupplierId,
            string SupplierName,
            string ProductName,
            int Quantity,
            string Status,
            DateTime CreatedAt
        );
    }
}