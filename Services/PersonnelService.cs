using Microsoft.EntityFrameworkCore;
using project_pharmacie.Areas.Admin.ViewModels;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class PersonnelService : IPersonnelService
{
    private readonly PharmacieDbContext _db;
    public PersonnelService(PharmacieDbContext db) => _db = db;

    public async Task<List<Personnel>> SearchAsync(string? q)
    {
        var query = _db.Personnels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p =>
                p.Nom.Contains(q) ||
                p.Login.Contains(q));
        }

        return await query
            .OrderByDescending(p => p.Nom)
            .ToListAsync();
    }

    public Task<Personnel?> GetAsync(string id)
        => _db.Personnels.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<ServiceResult<Personnel>> CreateAsync(PersonnelForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Nom))
            return ServiceResult<Personnel>.Fail("Nom obligatoire.");

        if (string.IsNullOrWhiteSpace(form.Login))
            return ServiceResult<Personnel>.Fail("Login obligatoire.");

        var login = form.Login.Trim();

        var loginExists = await _db.Utilisateurs.AnyAsync(u => u.Login == login);
        if (loginExists)
            return ServiceResult<Personnel>.Fail("Login déjà utilisé.");

        // On rattache au 1er admin existant (si présent)
        var adminId = await _db.Administrateurs
            .AsNoTracking()
            .Select(a => a.Id)
            .FirstOrDefaultAsync();

        var p = new Personnel
        {
            Nom = form.Nom.Trim(),
            Login = login,
            PasswordHash = "hash", // TODO: vraie auth plus tard
            Role = form.Poste.ToString(), // "Pharmacien", "Caissier", ...
            AdministrateurId = adminId
        };

        _db.Personnels.Add(p);
        await _db.SaveChangesAsync();
        return ServiceResult<Personnel>.Ok(p);
    }

    public async Task<ServiceResult> UpdateAsync(string id, PersonnelForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Nom))
            return ServiceResult.Fail("Nom obligatoire.");

        if (string.IsNullOrWhiteSpace(form.Login))
            return ServiceResult.Fail("Login obligatoire.");

        var p = await _db.Personnels.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
            return ServiceResult.Fail("Personnel introuvable.");

        var login = form.Login.Trim();

        var loginExists = await _db.Utilisateurs.AnyAsync(u => u.Login == login && u.Id != id);
        if (loginExists)
            return ServiceResult.Fail("Login déjà utilisé par un autre utilisateur.");

        p.Nom = form.Nom.Trim();
        p.Login = login;
        p.Role = form.Poste.ToString();

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        var p = await _db.Personnels.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
            return ServiceResult.Fail("Personnel introuvable.");

        _db.Personnels.Remove(p);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}