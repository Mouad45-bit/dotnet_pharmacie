using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using project_pharmacie.Areas.Admin.ViewModels;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

[Authorize(Roles = "ADMIN")]
public class CreateModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public CreateModel(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    [BindProperty]
    public PersonnelForm Form { get; set; } = new();

    // ADMIN / PERSONNEL
    public List<SelectListItem> RoleOptions { get; } = new()
    {
        new SelectListItem("Personnel", "PERSONNEL"),
        new SelectListItem("Admin", "ADMIN")
    };

    public void OnGet()
    {
        // valeur par défaut
        Form.Role = "PERSONNEL";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // validations
        if (string.IsNullOrWhiteSpace(Form.Nom))
            ModelState.AddModelError("Form.Nom", "Nom obligatoire.");

        if (string.IsNullOrWhiteSpace(Form.Login))
            ModelState.AddModelError("Form.Login", "Login obligatoire.");

        var role = (Form.Role ?? "").Trim().ToUpperInvariant();
        if (role is not ("ADMIN" or "PERSONNEL"))
            ModelState.AddModelError("Form.Role", "Rôle invalide (ADMIN ou PERSONNEL).");

        if (!ModelState.IsValid)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Veuillez corriger les erreurs du formulaire.";
            return Page();
        }

        var login = Form.Login.Trim();

        // On choisit une convention simple :
        // - UserName = login
        // - Email = login + "@local" (si tu n’utilises pas d’email réel)
        var email = $"{login}@local";

        // empêcher doublons (username ou email)
        var existingByUsername = await _users.FindByNameAsync(login);
        if (existingByUsername is not null)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Login déjà utilisé.";
            return Page();
        }

        var existingByEmail = await _users.FindByEmailAsync(email);
        if (existingByEmail is not null)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = "Email déjà utilisé.";
            return Page();
        }

        // s’assurer que les rôles existent
        if (!await _roles.RoleExistsAsync("ADMIN"))
            await _roles.CreateAsync(new IdentityRole("ADMIN"));

        if (!await _roles.RoleExistsAsync("PERSONNEL"))
            await _roles.CreateAsync(new IdentityRole("PERSONNEL"));

        // créer user
        var user = new ApplicationUser
        {
            UserName = login,
            Email = email,
            Nom = Form.Nom.Trim(),
            EmailConfirmed = true
        };

        // Mot de passe par défaut (à changer ensuite)
        // Important: doit respecter la policy Identity (majuscule/minuscule/chiffre etc.)
        const string defaultPassword = "Azerty@12345";

        var createRes = await _users.CreateAsync(user, defaultPassword);
        if (!createRes.Succeeded)
        {
            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = string.Join(" | ", createRes.Errors.Select(e => e.Description));
            return Page();
        }

        // assigner rôle
        var roleRes = await _users.AddToRoleAsync(user, role);
        if (!roleRes.Succeeded)
        {
            // rollback si ajout rôle échoue
            await _users.DeleteAsync(user);

            TempData["FlashType"] = "error";
            TempData["FlashMessage"] = string.Join(" | ", roleRes.Errors.Select(e => e.Description));
            return Page();
        }

        TempData["FlashType"] = "success";
        TempData["FlashMessage"] = $"Personnel ajouté avec succès. Mot de passe par défaut : {defaultPassword}";
        return RedirectToPage("/Personnel/Index", new { area = "Admin" });
    }
}