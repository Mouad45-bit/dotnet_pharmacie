using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_pharmacie.Areas.Staff.Pages.Sales
{
    public class HistoryModel : PageModel
    {
        private readonly IVenteService _venteService;

        public HistoryModel(IVenteService venteService) => _venteService = venteService;

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public string StatusFilter => (Status ?? "").Trim().ToUpperInvariant();
        public string Query => Q ?? "";

        public List<SaleRow> Sales { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var result = await _venteService.ListAsync(Query);

            if (!result.Success)
            {
                Sales = new();
                TempData["FlashType"] = "error";
                TempData["FlashMessage"] = result.Error ?? "Impossible de charger l'historique des ventes.";
                return;
            }

            Sales = result.Data!
                .Select(v => new SaleRow(
                    Id: v.Id,
                    CustomerName: v.CustomerName,
                    DrugName: v.DrugName,
                    Quantity: v.Quantity,
                    TotalPrice: v.TotalPrice,
                    Status: v.Status,
                    Date: v.Date
                ))
                .ToList();

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