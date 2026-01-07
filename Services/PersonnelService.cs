using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Areas.Admin.ViewModels;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class PersonnelService : IPersonnelService
{
    // Mot de passe par défaut pour les comptes créés via l’admin (MVP).
    // Tu pourras remplacer par "ResetPassword" plus tard.
    private const string DefaultPassword = "Admin@12345";

    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;

    public PersonnelService(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<List<Personnel>> SearchAsync(string? q)
    {
        var query = _users.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(u =>
                (u.Nom ?? "").Contains(q) ||
                (u.UserName ?? "").Contains(q) ||
                (u.Email ?? "").Contains(q));
        }

        var list = await query
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.UserName)
            .ToListAsync();

        var result = new List<Personnel>(list.Count);
        foreach (var u in list)
        {
            var role = await GetMainRoleAsync(u);
            result.Add(ToPersonnelVm(u, role));
        }

        return result;
    }

    public async Task<Personnel?> GetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        var u = await _users.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (u is null) return null;

        var role = await GetMainRoleAsync(u);
        return ToPersonnelVm(u, role);
    }

    public async Task<ServiceResult<Personnel>> CreateAsync(PersonnelForm form)
    {
        var nom = (form.Nom ?? "").Trim();
        var login = (form.Login ?? "").Trim();
        var role = (form.Role ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nom))
            return ServiceResult<Personnel>.Fail("Nom obligatoire.");

        if (string.IsNullOrWhiteSpace(login))
            return ServiceResult<Personnel>.Fail("Login obligatoire.");

        if (role is not ("ADMIN" or "PERSONNEL"))
            return ServiceResult<Personnel>.Fail("Rôle invalide (ADMIN ou PERSONNEL).");

        // S'assure que les rôles existent (sinon AddToRole échoue)
        await EnsureRoleExistsAsync("ADMIN");
        await EnsureRoleExistsAsync("PERSONNEL");

        // Vérif login unique
        var existing = await _users.FindByNameAsync(login);
        if (existing is not null)
            return ServiceResult<Personnel>.Fail("Login déjà utilisé.");

        var user = new ApplicationUser
        {
            Nom = nom,
            UserName = login,
            Email = $"{login}@local.test", // optionnel
            EmailConfirmed = true
        };

        var create = await _users.CreateAsync(user, DefaultPassword);
        if (!create.Succeeded)
            return ServiceResult<Personnel>.Fail(string.Join(" | ", create.Errors.Select(e => e.Description)));

        var addRole = await _users.AddToRoleAsync(user, role);
        if (!addRole.Succeeded)
            return ServiceResult<Personnel>.Fail(string.Join(" | ", addRole.Errors.Select(e => e.Description)));

        return ServiceResult<Personnel>.Ok(ToPersonnelVm(user, role));
    }

    public async Task<ServiceResult> UpdateAsync(string id, PersonnelForm form)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ServiceResult.Fail("Id invalide.");

        var nom = (form.Nom ?? "").Trim();
        var login = (form.Login ?? "").Trim();
        var role = (form.Role ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(nom))
            return ServiceResult.Fail("Nom obligatoire.");

        if (string.IsNullOrWhiteSpace(login))
            return ServiceResult.Fail("Login obligatoire.");

        if (role is not ("ADMIN" or "PERSONNEL"))
            return ServiceResult.Fail("Rôle invalide (ADMIN ou PERSONNEL).");

        var user = await _users.FindByIdAsync(id);
        if (user is null)
            return ServiceResult.Fail("Personnel introuvable.");

        // Vérif login unique (si change)
        if (!string.Equals(user.UserName, login, StringComparison.OrdinalIgnoreCase))
        {
            var byName = await _users.FindByNameAsync(login);
            if (byName is not null && byName.Id != user.Id)
                return ServiceResult.Fail("Login déjà utilisé par un autre utilisateur.");

            var setUserName = await _users.SetUserNameAsync(user, login);
            if (!setUserName.Succeeded)
                return ServiceResult.Fail(string.Join(" | ", setUserName.Errors.Select(e => e.Description)));
        }

        user.Nom = nom;

        var update = await _users.UpdateAsync(user);
        if (!update.Succeeded)
            return ServiceResult.Fail(string.Join(" | ", update.Errors.Select(e => e.Description)));

        // Update du rôle principal (ADMIN/PERSONNEL)
        await EnsureRoleExistsAsync("ADMIN");
        await EnsureRoleExistsAsync("PERSONNEL");

        var currentRoles = await _users.GetRolesAsync(user);
        var toRemove = currentRoles.Where(r => r is "ADMIN" or "PERSONNEL").ToList();

        if (toRemove.Count > 0)
        {
            var remove = await _users.RemoveFromRolesAsync(user, toRemove);
            if (!remove.Succeeded)
                return ServiceResult.Fail(string.Join(" | ", remove.Errors.Select(e => e.Description)));
        }

        var add = await _users.AddToRoleAsync(user, role);
        if (!add.Succeeded)
            return ServiceResult.Fail(string.Join(" | ", add.Errors.Select(e => e.Description)));

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ServiceResult.Fail("Id invalide.");

        var user = await _users.FindByIdAsync(id);
        if (user is null)
            return ServiceResult.Fail("Personnel introuvable.");

        // Sécurité : ne supprime pas le dernier admin
        var roles = await _users.GetRolesAsync(user);
        if (roles.Contains("ADMIN"))
        {
            var admins = await _users.GetUsersInRoleAsync("ADMIN");
            if (admins.Count <= 1)
                return ServiceResult.Fail("Impossible de supprimer le dernier administrateur.");
        }

        var del = await _users.DeleteAsync(user);
        if (!del.Succeeded)
            return ServiceResult.Fail(string.Join(" | ", del.Errors.Select(e => e.Description)));

        return ServiceResult.Ok();
    }

    // ----- Helpers -----

    private Personnel ToPersonnelVm(ApplicationUser u, string role)
        => new Personnel
        {
            Id = u.Id,
            Nom = string.IsNullOrWhiteSpace(u.Nom) ? (u.UserName ?? "") : u.Nom,
            Login = u.UserName ?? "",
            Role = role,

            // champ legacy (on ne l’utilise plus avec Identity)
            PasswordHash = ""
        };

    private async Task<string> GetMainRoleAsync(ApplicationUser u)
    {
        var roles = await _users.GetRolesAsync(u);
        var main = roles.FirstOrDefault(r => r is "ADMIN" or "PERSONNEL");
        return main ?? "PERSONNEL";
    }

    private async Task EnsureRoleExistsAsync(string role)
    {
        if (!await _roles.RoleExistsAsync(role))
            await _roles.CreateAsync(new IdentityRole(role));
    }
}