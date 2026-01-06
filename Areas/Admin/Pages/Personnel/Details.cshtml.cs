using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class DetailsModel : PageModel
{
    private readonly IPersonnelService _service;

    public DetailsModel(IPersonnelService service) => _service = service;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    public Models.Personnel? Item { get; private set; }
    public bool Found { get; private set; }

    public async Task OnGetAsync()
    {
        Item = await _service.GetAsync(Id);
        Found = Item is not null;
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var res = await _service.DeleteAsync(id);

        TempData["FlashType"] = res.Success ? "success" : "error";
        TempData["FlashMessage"] = res.Success
            ? "Personnel supprimé avec succès."
            : (res.Error ?? "Erreur lors de la suppression.");

        return RedirectToPage("/Personnel/Index", new { area = "Admin" });
    }
}