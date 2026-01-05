using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Staff.Pages.Products;

public class EditModel : PageModel
{
    private readonly IProduitService _produitService;

    public EditModel(IProduitService produitService) => _produitService = produitService;

    // L'id dans l'URL: @page "{id}" => id = Reference (string)
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public Produit Produit { get; set; } = new();

    public bool Found { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(string id)
    {
        Id = id;

        var existing = await _produitService.GetByReferenceAsync(id);
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
        if (string.IsNullOrWhiteSpace(Produit.Reference))
        {
            Found = false;
            ErrorMessage = "Produit introuvable.";
            return Page();
        }

        // Vérifier existence
        var existing = await _produitService.GetByReferenceAsync(Produit.Reference);
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

        var result = await _produitService.UpdateAsync(Produit);
        if (!result.Success)
        {
            ErrorMessage = result.Error ?? "Impossible de modifier le produit.";
            return Page();
        }

        TempData["Toast.Success"] = $"Produit '{Produit.Nom}' modifié.";
        return RedirectToPage("/Products/Index", new { area = "Staff" });
    }
}