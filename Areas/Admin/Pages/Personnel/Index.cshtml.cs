using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Areas.Admin.Services;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class IndexModel : PageModel
{
    public List<Services.Personnel> Items { get; private set; } = new();

    public void OnGet()
    {
        Items = PersonnelStore.All();
    }
}
