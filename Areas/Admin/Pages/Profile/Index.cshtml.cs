using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Admin.Pages.Profile;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public string AdminName { get; private set; } = "";
    public string AdminEmail { get; private set; } = "";
    public string LastLogin { get; private set; } = "";

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        AdminName = user?.UserName ?? "Admin";
        AdminEmail = user?.Email ?? "";
        LastLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}