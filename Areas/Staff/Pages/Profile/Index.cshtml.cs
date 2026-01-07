using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Profile;

[Authorize(Roles = "ADMIN,PERSONNEL")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;

    public IndexModel(UserManager<ApplicationUser> users)
        => _users = users;

    public bool Found { get; private set; }

    public string Username { get; private set; } = "";
    public string Role { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string EmployeeId { get; private set; } = "";

    [BindProperty]
    public string? CurrentPassword { get; set; }

    [BindProperty]
    public string? NewPassword { get; set; }

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        await LoadUserAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null)
        {
            Found = false;
            Message = "Utilisateur introuvable ou non connecté.";
            return Page();
        }

        // Si aucun champ password => juste reload infos
        if (string.IsNullOrWhiteSpace(CurrentPassword) && string.IsNullOrWhiteSpace(NewPassword))
        {
            await LoadUserAsync(user);
            return Page();
        }

        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            Message = "Veuillez remplir l’ancien et le nouveau mot de passe.";
            await LoadUserAsync(user);
            return Page();
        }

        var res = await _users.ChangePasswordAsync(user, CurrentPassword, NewPassword);

        if (!res.Succeeded)
        {
            Message = string.Join(" | ", res.Errors.Select(e => e.Description));
            await LoadUserAsync(user);
            return Page();
        }

        Message = "Mot de passe modifié avec succès.";
        CurrentPassword = null;
        NewPassword = null;

        await LoadUserAsync(user);
        return Page();
    }

    private async Task LoadUserAsync(ApplicationUser? user = null)
    {
        user ??= await _users.GetUserAsync(User);

        if (user is null)
        {
            Found = false;
            Username = "";
            Role = "";
            Email = "";
            EmployeeId = "";
            return;
        }

        Found = true;

        Username = string.IsNullOrWhiteSpace(user.Nom)
            ? (user.UserName ?? "")
            : user.Nom;

        Email = user.Email ?? (user.UserName ?? "");
        EmployeeId = user.Id;

        var roles = await _users.GetRolesAsync(user);
        Role = roles.FirstOrDefault(r => r is "ADMIN" or "PERSONNEL") ?? "PERSONNEL";
    }
}