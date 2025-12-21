using Microsoft.AspNetCore.Mvc.RazorPages;

namespace project_pharmacie.Areas.Admin.Pages.Profile;

public class IndexModel : PageModel
{
    public string AdminName { get; private set; } = "Admin Demo";
    public string AdminEmail { get; private set; } = "admin@pharmacie.local";
    public string LastLogin { get; private set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

    public void OnGet()
    {
        // Plus tard : lire depuis User.Claims / Identity
    }
}