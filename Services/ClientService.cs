using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class ClientService : IClientService
{
    private readonly PharmacieDbContext _db;
    public ClientService(PharmacieDbContext db) => _db = db;

    public async Task<(List<Client> items, int total)> SearchAsync(string? q)
    {
        var query = _db.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.Name, $"%{q}%") ||
                EF.Functions.Like(c.Email, $"%{q}%")
            );
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(c => c.Name).ToListAsync();
        return (items, total);
    }

    public Task<Client?> GetByIdAsync(string id)
        => _db.Clients.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ServiceResult<Client>> CreateAsync(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.Name))
            return ServiceResult<Client>.Fail("Le nom est requis.");

        client.Name = client.Name.Trim();
        client.Email = client.Email?.Trim();
        client.Phone = client.Phone?.Trim();

        // Statut initial
        client.LoyaltyPoints = Math.Max(0, client.LoyaltyPoints);
        client.Status = client.LoyaltyPoints >= 120 ? "Or"
                     : client.LoyaltyPoints >= 60 ? "Argent"
                     : "Nouveau";

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        return ServiceResult<Client>.Ok(client);
    }

    public async Task<ServiceResult> UpdateAsync(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.Id))
            return ServiceResult.Fail("ID client invalide.");

        if (string.IsNullOrWhiteSpace(client.Name))
            return ServiceResult.Fail("Le nom est requis.");

        var existing = await _db.Clients.FindAsync(client.Id);
        if (existing is null)
            return ServiceResult.Fail("Client introuvable.");

        existing.Name = client.Name.Trim();
        existing.Email = client.Email?.Trim();
        existing.Phone = client.Phone?.Trim();
        existing.PersonalizedOffer = client.PersonalizedOffer;
        existing.LoyaltyPoints = Math.Max(0, client.LoyaltyPoints);

        // recalcul statut (comme ton code actuel)
        existing.Status = existing.LoyaltyPoints >= 120 ? "Or"
                       : existing.LoyaltyPoints >= 60 ? "Argent"
                       : "Nouveau";

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client is null) return ServiceResult.Fail("Client introuvable.");

        // (Option) bloquer si ventes existent
        // if (await _db.Ventes.AnyAsync(v => v.ClientId == id))
        //     return ServiceResult.Fail("Suppression impossible : ce client a des ventes.");

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<Client>> CreateAsync(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.Name))
            return ServiceResult<Client>.Fail("Le nom est requis.");

        client.Name = client.Name.Trim();
        client.Email = client.Email?.Trim();
        client.Phone = client.Phone?.Trim();

        client.LoyaltyPoints = Math.Max(0, client.LoyaltyPoints);

        // statut cohérent dès la création (comme ton code)
        client.Status = client.LoyaltyPoints >= 120 ? "Or"
                     : client.LoyaltyPoints >= 60 ? "Argent"
                     : "Nouveau";

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        return ServiceResult<Client>.Ok(client);
    }
}