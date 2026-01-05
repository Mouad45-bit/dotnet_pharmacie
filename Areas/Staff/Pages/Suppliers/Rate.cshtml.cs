using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Suppliers;

public class RateModel : PageModel
{
    private readonly PharmacieDbContext _db;

    public RateModel(PharmacieDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string SupplierId { get; set; } = "";

    [BindProperty]
    public RateForm Form { get; set; } = new();

    public bool Found { get; private set; }
    public string SupplierName { get; private set; } = "-";
    public double CurrentRating { get; private set; }
    public int CurrentRatingsCount { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadSupplierOrNotFoundAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadSupplierOrNotFoundAsync();
        if (!Found)
        {
            ErrorMessage = "Fournisseur introuvable.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Veuillez corriger les champs en erreur.";
            return Page();
        }

        var supplier = await _db.Fournisseurs.FindAsync(SupplierId);
        if (supplier is null)
        {
            ErrorMessage = "Fournisseur introuvable.";
            return Page();
        }

        // MVP (safe) : on remplace la note globale
        supplier.NoteGlobale = Form.Rating;
        await _db.SaveChangesAsync();

        TempData["Toast.Success"] = $"Note enregistrée : {Form.Rating}/5 pour {SupplierName}.";
        return RedirectToPage("/Suppliers/Index", new { area = "Staff" });
    }

    private async Task LoadSupplierOrNotFoundAsync()
    {
        var s = await _db.Fournisseurs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == SupplierId);
        if (s is null)
        {
            Found = false;
            return;
        }

        Found = true;
        SupplierName = s.Nom;
        CurrentRating = s.NoteGlobale;
        CurrentRatingsCount = await _db.Commandes.CountAsync(c => c.FournisseurId == s.Id);

        if (Form.Rating == 0)
            Form.Rating = 5;
    }

    public class RateForm
    {
        [Range(1, 5, ErrorMessage = "La note doit être entre 1 et 5")]
        public int Rating { get; set; }

        [StringLength(200, ErrorMessage = "Commentaire trop long (max 200)")]
        public string? Comment { get; set; }
    }
}