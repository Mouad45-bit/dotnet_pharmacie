using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace project_pharmacie.Areas.Staff.Pages.Sales
{
    public class HistoryModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public string StatusFilter => (Status ?? "").Trim().ToUpperInvariant();
        public string Query => Q ?? "";

        public List<SaleRow> Sales { get; private set; } = new();

        public void OnGet()
        {
            var all = MockSales();

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                all = all
                    .Where(s => (s.Status ?? "")
                        .Equals(StatusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(Query))
            {
                var q = Query.Trim().ToLowerInvariant();
                all = all
                    .Where(s =>
                        s.CustomerName.ToLowerInvariant().Contains(q) ||
                        s.DrugName.ToLowerInvariant().Contains(q))
                    .ToList();
            }

            Sales = all
                .OrderByDescending(s => s.Date)
                .ToList();
        }

        public (string cls, string label) GetStatusBadge(string status)
        {
            return (status ?? "").ToUpperInvariant() switch
            {
                "PAID" => ("bg-emerald-50 text-emerald-700 ring-emerald-200", "Payée"),
                "CANCELLED" => ("bg-red-50 text-red-700 ring-red-200", "Annulée"),
                _ => ("bg-amber-50 text-amber-700 ring-amber-200", "En attente"),
            };
        }

        private List<SaleRow> MockSales()
        {
            var now = DateTime.Now;

            return new List<SaleRow>
            {
                new SaleRow(201, "Jean Dupont", "Doliprane 1g", 2, 37.00m, "PAID",      now.AddDays(-1).AddHours(-2)),
                new SaleRow(202, "Marie Curie", "Vitamine C",  1, 45.00m, "PENDING",   now.AddDays(-2).AddHours(-5)),
                new SaleRow(203, "Paul Atreides","Aerius",     1, 79.90m, "CANCELLED", now.AddDays(-7).AddHours(-1)),
                new SaleRow(204, "Sara El Amrani","Smecta",    3, 102.00m,"PAID",      now.AddDays(-10).AddHours(-3)),
            };
        }

        public record SaleRow(
            int Id,
            string CustomerName,
            string DrugName,
            int Quantity,
            decimal TotalPrice,
            string Status,
            DateTime Date
        );
    }
}