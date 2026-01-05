using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Clients
{
    public class IndexModel : PageModel
    {
        // --- Recherche (comme Admin/Personnel) ---
        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public int Total { get; set; }

        public List<Client> Clients { get; set; } = new();

        // --- Flash (comme Admin/Personnel) ---
        [TempData] public string? FlashMessage { get; set; }
        [TempData] public string? FlashType { get; set; } // success | error | info

        // --- Store mock persistant (pour que Delete fonctionne vraiment) ---
        private static readonly object _lock = new();
        private static List<Client> _store = new()
        {
            new Client { Id = "1", Name = "Jean Dupont", Email = "jean.dupont@email.com", LoyaltyPoints = 120, Status = "Or",     PersonalizedOffer = "-10% Gamme Bio" },
            new Client { Id = "2", Name = "Marie Curie", Email = "marie.curie@science.org", LoyaltyPoints = 45,  Status = "Argent", PersonalizedOffer = "" },
            new Client { Id = "3", Name = "Paul Atreides", Email = "paul@dune.com", LoyaltyPoints = 5,  Status = "Nouveau", PersonalizedOffer = "" }
        };

        public void OnGet()
        {
            List<Client> query;

            lock (_lock)
            {
                query = _store.ToList();
            }

            // Filtrage recherche (Nom / Email)
            if (!string.IsNullOrWhiteSpace(Q))
            {
                var q = Q.Trim();
                query = query
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.Email) && c.Email.Contains(q, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }

            Total = query.Count;
            Clients = query;
        }

        public IActionResult OnPostDelete(string id, string? q)
        {
            // garder le q après suppression (comme Admin)
            Q = q;

            Client? removed = null;

            lock (_lock)
            {
                removed = _store.FirstOrDefault(c => c.Id == id);
                if (removed != null)
                {
                    _store.Remove(removed);
                }
            }

            if (removed == null)
            {
                FlashType = "error";
                FlashMessage = "Client introuvable (déjà supprimé ou ID invalide).";
            }
            else
            {
                FlashType = "success";
                FlashMessage = $"Client “{removed.Name}” supprimé avec succès.";
            }

            return RedirectToPage("/Clients/Index", new { area = "Staff", q = Q });
        }
    }
}