using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Staff.Pages.Clients;

public class EditModel : PageModel
{
    private readonly IClientService _clients;

    public EditModel(IClientService clients) => _clients = clients;

    [BindProperty]
    public Client Client { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var client = await _clients.GetByIdAsync(id);
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

        // On délègue l’update au service (validation + recalcul status)
        var res = await _clients.UpdateAsync(Client);

        if (!res.Success)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = res.Error ?? "Erreur lors de la mise à jour.";
            return RedirectToPage("./Index");
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Dossier client mis à jour.";
        return RedirectToPage("./Index");
    }
}