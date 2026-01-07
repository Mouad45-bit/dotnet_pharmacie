using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using project_pharmacie.Areas.Admin.ViewModels;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

[Authorize(Roles = "ADMIN")]
public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public EditModel(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    [BindProperty]
    public PersonnelForm Form { get; set; } = new();

    public bool Found { get; private set; }

    // ADMIN / PERSONNEL
    public List<SelectListItem> RoleOptions { get; } = new()
    {
        new SelectListItem("Personnel", "PERSONNEL"),
        new SelectListItem("Admin", "ADMIN")
    };

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Found = false;
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Id invalide.";
            return Page();
        }

        var user = await _users.FindByIdAsync(Id);
        if (user is null)
        {
            Found = false;
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Personnel introuvable.";
            return Page();
        }

        Found = true;

        // rôle courant (ADMIN/PERSONNEL), fallback PERSONNEL
        var roles = await _users.GetRolesAsync(user);
        var currentRole = roles.FirstOrDefault()?.Trim().ToUpperInvariant();
        if (currentRole is not ("ADMIN" or "PERSONNEL"))
            currentRole = "PERSONNEL";

        Form = new PersonnelForm
        {
            Nom = user.Nom ?? "",
            Login = user.UserName ?? "",
            Role = currentRole
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Found = true;

        if (string.IsNullOrWhiteSpace(Id))
            ModelState.AddModelError("", "Id invalide.");

        if (string.IsNullOrWhiteSpace(Form.Nom))
            ModelState.AddModelError("Form.Nom", "Nom obligatoire.");

        if (string.IsNullOrWhiteSpace(Form.Login))
            ModelState.AddModelError("Form.Login", "Login obligatoire.");

        var newRole = (Form.Role ?? "").Trim().ToUpperInvariant();
        if (newRole is not ("ADMIN" or "PERSONNEL"))
            ModelState.AddModelError("Form.Role", "Rôle invalide (ADMIN ou PERSONNEL).");

        if (!ModelState.IsValid)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Veuillez corriger les erreurs du formulaire.";
            return Page();
        }

        var user = await _users.FindByIdAsync(Id);
        if (user is null)
        {
            Found = false;
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Personnel introuvable.";
            return Page();
        }

        // Normalisation login
        var login = Form.Login.Trim();

        // Vérifier unicité login (UserName)
        var existingUser = await _users.FindByNameAsync(login);
        if (existingUser is not null && existingUser.Id != user.Id)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Login déjà utilisé par un autre utilisateur.";
            return Page();
        }

        // S’assurer que les rôles existent
        if (!await _roles.RoleExistsAsync("ADMIN"))
            await _roles.CreateAsync(new IdentityRole("ADMIN"));
        if (!await _roles.RoleExistsAsync("PERSONNEL"))
            await _roles.CreateAsync(new IdentityRole("PERSONNEL"));

        // Rôle actuel
        var currentRoles = await _users.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault()?.Trim().ToUpperInvariant();
        if (currentRole is not ("ADMIN" or "PERSONNEL"))
            currentRole = "PERSONNEL";

        // Protection : empêcher de retirer le dernier ADMIN
        if (currentRole == "ADMIN" && newRole != "ADMIN")
        {
            var admins = await _users.GetUsersInRoleAsync("ADMIN");
            if (admins.Count <= 1)
            {
                TempData["FlashType"] = "error";
                TempData["FlashMessage"] = "Impossible : vous ne pouvez pas retirer le dernier administrateur.";
                return Page();
            }
        }

        // Update user fields
        user.Nom = Form.Nom.Trim();
        user.UserName = login;

        // si tu utilises Email pour login local, on le garde cohérent
        // (si tu n’utilises pas Email, tu peux supprimer ces 2 lignes)
        user.Email = $"{login}@local";
        user.NormalizedEmail = _users.NormalizeEmail(user.Email);

        var updRes = await _users.UpdateAsync(user);
        if (!updRes.Succeeded)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = string.Join(" | ", updRes.Errors.Select(e => e.Description));
            return Page();
        }

        // Changement rôle si besoin
        if (currentRole != newRole)
        {
            if (currentRole is "ADMIN" or "PERSONNEL")
            {
                var remRes = await _users.RemoveFromRoleAsync(user, currentRole);
                if (!remRes.Succeeded)
                {
                    TempData["FlashType"] = "error";
                    TempData["FlashMessage"] = string.Join(" | ", remRes.Errors.Select(e => e.Description));
                    return Page();
                }
            }

            var addRes = await _users.AddToRoleAsync(user, newRole);
            if (!addRes.Succeeded)
            {
                TempData["FlashType"] = "error";
                TempData["FlashMessage"] = string.Join(" | ", addRes.Errors.Select(e => e.Description));
                return Page();
            }
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = "Modifications enregistrées.";
        return RedirectToPage("/Personnel/Index", new { area = "Admin" });
    }
}