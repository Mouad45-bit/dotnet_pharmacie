using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Clients;

public class CreateModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public CreateModel(PharmacieDbContext db) => _db = db;

    [BindProperty]
    public Client Client { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // statut cohérent dès la création
        Client.Status = Client.LoyaltyPoints >= 120 ? "Or"
                     : Client.LoyaltyPoints >= 60 ? "Argent"
                     : "Nouveau";

        _db.Clients.Add(Client);
        await _db.SaveChangesAsync();

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = $"Client {Client.Name} créé avec succès.";
        return RedirectToPage("./Index");
    }
}
