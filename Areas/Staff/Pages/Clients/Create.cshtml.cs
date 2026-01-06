using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Staff.Pages.Clients;

public class CreateModel : PageModel
{
    private readonly IClientService _clients;

    public CreateModel(IClientService clients) => _clients = clients;

    [BindProperty]
    public Client Client { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var res = await _clients.CreateAsync(Client);
        if (!res.Success)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = res.Error ?? "Erreur lors de la création du client.";
            return Page();
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = $"Client {res.Data?.Name ?? Client.Name} créé avec succès.";
        return RedirectToPage("./Index");
    }
}