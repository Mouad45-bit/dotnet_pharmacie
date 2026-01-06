using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace project_pharmacie.Areas.Staff.Pages.Orders
{
    public class HistoryModel : PageModel
    {
        private readonly ICommandeService _commandeService;

        public HistoryModel(ICommandeService commandeService) => _commandeService = commandeService;

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public string StatusFilter => (Status ?? "").Trim().ToUpperInvariant();
        public string Query => Q ?? "";

        public List<OrderRow> Rows { get; private set; } = new();

        public async Task OnGetAsync()
        {
            // Lecture via Service (pour centraliser)
            var result = await _commandeService.ListAsync(Query);

            if (!result.Success)
            {
                Rows = new();
                TempData["FlashType"] = "error";
                TempData["FlashMessage"] = result.Error ?? "Impossible de charger l'historique.";
                return;
            }

            Rows = result.Data!
                .Select(o => new OrderRow(
                    Id: o.Id,
                    SupplierId: o.SupplierId,
                    SupplierName: o.SupplierName,
                    ProductName: o.ProductName,
                    Quantity: o.Quantity,
                    Status: ApplyDerivedStatus(o.CreatedAt),
                    CreatedAt: o.CreatedAt,
                    Total: o.Total
                ))
                .ToList();

            // Filtre statut (statut dérivé)
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                Rows = Rows
                    .Where(r => r.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        // Ici on garde la logique de statut “simulé” côté UI,
        // car on n’a pas de colonne Status en DB.
        private static string ApplyDerivedStatus(DateTime createdAt)
        {
            var age = DateTime.Now - createdAt;
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