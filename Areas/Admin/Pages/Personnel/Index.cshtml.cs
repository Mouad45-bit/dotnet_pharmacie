using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

[Authorize(Roles = "ADMIN")]
public class IndexModel : PageModel
{
    private readonly IPersonnelService _service;

    public IndexModel(IPersonnelService service) => _service = service;

    public List<Models.Personnel> Items { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    public int Total => Items.Count;

    public async Task OnGetAsync()
    {
        Items = await _service.SearchAsync(Q);
    }
}