using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Clients;

public class EditModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public EditModel(PharmacieDbContext db) => _db = db;

    [BindProperty]
    public Client Client { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client is null)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Client introuvable.";
            return RedirectToPage("./Index");
        }

        Client = client;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _db.Clients.FindAsync(Client.Id);
        if (existing is null)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Client introuvable.";
            return RedirectToPage("./Index");
        }

        existing.Name = Client.Name;
        existing.Email = Client.Email;
        existing.Phone = Client.Phone;
        existing.LoyaltyPoints = Client.LoyaltyPoints;
        existing.PersonalizedOffer = Client.PersonalizedOffer;

        // recalcul statut
        existing.Status = existing.LoyaltyPoints >= 120 ? "Or"
                       : existing.LoyaltyPoints >= 60 ? "Argent"
                       : "Nouveau";

        await _db.SaveChangesAsync();

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Dossier client mis à jour.";
        return RedirectToPage("./Index");
    }
}