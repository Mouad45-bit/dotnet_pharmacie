using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Products;

public class CreateModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public CreateModel(PharmacieDbContext db) => _db = db;

    [BindProperty]
    public Produit Produit { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Veuillez corriger les champs en erreur.";
            return Page();
        }

        var exists = await _db.Produits.AnyAsync(p => p.Reference == Produit.Reference);
        if (exists)
        {
            ModelState.AddModelError("Produit.Reference", "Cette référence existe déjà.");
            ErrorMessage = "Référence déjà utilisée.";
            return Page();
        }

        _db.Produits.Add(Produit);
        await _db.SaveChangesAsync();

        TempData["Toast.Success"] = $"Produit '{Produit.Nom}' ajouté.";
        return RedirectToPage("/Products/Index", new { area = "Staff" });
    }
}