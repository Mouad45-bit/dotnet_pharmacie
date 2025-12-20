using System.Collections.Concurrent;

namespace project_pharmacie.Areas.Admin.Services;

public static class PersonnelStore
{
    private static readonly ConcurrentDictionary<string, Personnel> _db = new();

    static PersonnelStore()
    {
        Add(new PersonnelForm
        {
            FullName = "Admin Demo",
            Email = "admin@pharmacie.local",
            Phone = "0600000000",
            Role = PersonnelRole.Admin,
            IsActive = true
        });

        Add(new PersonnelForm
        {
            FullName = "Sara Pharmacien",
            Email = "sara@pharmacie.local",
            Phone = "0611111111",
            Role = PersonnelRole.Pharmacien,
            IsActive = true
        });
    }

    public static List<Personnel> All(string? q = null)
    {
        var list = _db.Values.OrderByDescending(x => x.CreatedAt).ToList();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim().ToLowerInvariant();
            list = list.Where(p =>
                    p.FullName.ToLowerInvariant().Contains(q) ||
                    p.Email.ToLowerInvariant().Contains(q) ||
                    p.Phone.ToLowerInvariant().Contains(q))
                .ToList();
        }

        return list;
    }

    public static Personnel? Get(string id)
        => _db.TryGetValue(id, out var p) ? p : null;

    public static (bool ok, string? error, Personnel? created) Add(PersonnelForm form)
    {
        if (string.IsNullOrWhiteSpace(form.FullName))
            return (false, "Nom complet obligatoire.", null);

        if (string.IsNullOrWhiteSpace(form.Email))
            return (false, "Email obligatoire.", null);

        var emailLower = form.Email.Trim().ToLowerInvariant();
        if (_db.Values.Any(x => x.Email.Trim().ToLowerInvariant() == emailLower))
            return (false, "Email déjà utilisé.", null);

        var p = new Personnel
        {
            Id = Guid.NewGuid().ToString("N"),
            FullName = form.FullName.Trim(),
            Email = form.Email.Trim(),
            Phone = form.Phone.Trim(),
            Role = form.Role,
            IsActive = form.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db[p.Id] = p;
        return (true, null, p);
    }

    public static (bool ok, string? error) Update(string id, PersonnelForm form)
    {
        var existing = Get(id);
        if (existing is null) return (false, "Personnel introuvable.");

        if (string.IsNullOrWhiteSpace(form.FullName))
            return (false, "Nom complet obligatoire.");

        if (string.IsNullOrWhiteSpace(form.Email))
            return (false, "Email obligatoire.");

        var emailLower = form.Email.Trim().ToLowerInvariant();
        if (_db.Values.Any(x => x.Id != id && x.Email.Trim().ToLowerInvariant() == emailLower))
            return (false, "Email déjà utilisé par un autre compte.");

        existing.FullName = form.FullName.Trim();
        existing.Email = form.Email.Trim();
        existing.Phone = form.Phone.Trim();
        existing.Role = form.Role;
        existing.IsActive = form.IsActive;

        _db[id] = existing;
        return (true, null);
    }

    public static (bool ok, string? error) Delete(string id)
    {
        if (!_db.ContainsKey(id))
            return (false, "Personnel introuvable.");

        _db.TryRemove(id, out _);
        return (true, null);
    }
}
