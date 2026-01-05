using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Products;

public class EditModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public EditModel(PharmacieDbContext db) => _db = db;

    // L'id dans l'URL: @page "{id}" => id = Reference (string)
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    // Bind direct sur le modèle EF
    [BindProperty]
    public Produit Produit { get; set; } = new();

    public bool Found { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(string id)
    {
        Id = id;

        var existing = await _db.Produits.FindAsync(id);
        if (existing is null)
        {
            Found = false;
            return;
        }

        Found = true;
        Produit = existing;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Recharger "Found" (utile si l'utilisateur POST un id invalide)
        if (string.IsNullOrWhiteSpace(Produit.Reference))
        {
            Found = false;
            ErrorMessage = "Produit introuvable.";
            return Page();
        }

        var existing = await _db.Produits.FindAsync(Produit.Reference);
        Found = existing is not null;

        if (!Found)
        {
            ErrorMessage = "Produit introuvable.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Veuillez corriger les champs en erreur.";
            return Page();
        }

        // Update (Reference = PK, on ne la modifie pas)
        existing!.Nom = Produit.Nom;
        existing.Prix = Produit.Prix;
        existing.Quantite = Produit.Quantite;
        existing.DatePeremption = Produit.DatePeremption;

        await _db.SaveChangesAsync();

        TempData["Toast.Success"] = $"Produit '{existing.Nom}' modifié.";
        return RedirectToPage("/Products/Index", new { area = "Staff" });
    }
}