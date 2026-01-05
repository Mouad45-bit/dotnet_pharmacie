using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Staff.Pages.Sales;

public class CreateModel : PageModel
{
    [BindProperty]
    public SaleForm NewSale { get; set; } = new();

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        // pour l’instant: mock ok
        // plus tard: on mappe vers Vente + VenteLigne + Client/Produit en DB

        TempData["Message"] = $"Vente enregistrée pour {NewSale.CustomerName}.";
        return RedirectToPage("./History");
    }

    public class SaleForm
    {
        [Required] public string CustomerName { get; set; } = "";
        [Required] public string DrugName { get; set; } = "";
        [Range(1, 9999)] public int Quantity { get; set; } = 1;
        [Range(0, 999999)] public decimal TotalPrice { get; set; } = 0m;
    }
}