using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

[Authorize(Roles = "ADMIN")]
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
}