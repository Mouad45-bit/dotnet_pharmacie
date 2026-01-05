using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Staff.Pages.Products;

public class CreateModel : PageModel
{
    private readonly IProduitService _produitService;

    public CreateModel(IProduitService produitService) => _produitService = produitService;

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

        var result = await _produitService.CreateAsync(Produit);
        if (!result.Success)
        {
            // Si c'est une erreur de référence, on colle aussi dans ModelState (UI)
            if (!string.IsNullOrWhiteSpace(result.Error) && result.Error.Contains("référence", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Produit.Reference", result.Error);
            }

            ErrorMessage = result.Error ?? "Impossible de créer le produit.";
            return Page();
        }

        TempData["Toast.Success"] = $"Produit '{Produit.Nom}' ajouté.";
        return RedirectToPage("/Products/Index", new { area = "Staff" });
    }
}