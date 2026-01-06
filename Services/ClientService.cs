using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class ClientService : IClientService
{
    private readonly PharmacieDbContext _db;

    public ClientService(PharmacieDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Client> items, int total)> SearchAsync(string? q)
    {
        q = (q ?? "").Trim();

        IQueryable<Client> query = _db.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(c =>
                c.Name.Contains(q) ||
                (c.Email != null && c.Email.Contains(q)) ||
                (c.Phone != null && c.Phone.Contains(q)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Client?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        return await _db.Clients
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ServiceResult<Client>> CreateAsync(Client client)
    {
        if (client is null)
            return ServiceResult<Client>.Fail("Client invalide.");

        var name = (client.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult<Client>.Fail("Nom obligatoire.");

        var entity = new Client
        {
            Name = name,
            Email = (client.Email ?? "").Trim(),
            Phone = (client.Phone ?? "").Trim(),
            LoyaltyPoints = client.LoyaltyPoints,
            Status = string.IsNullOrWhiteSpace(client.Status) ? "Nouveau" : client.Status.Trim(),
            PersonalizedOffer = (client.PersonalizedOffer ?? "").Trim()
        };

        _db.Clients.Add(entity);
        await _db.SaveChangesAsync();

        return ServiceResult<Client>.Ok(entity);
    }

    // ⚠️ signature attendue par TON interface : UpdateAsync(Client client)
    public async Task<ServiceResult> UpdateAsync(Client client)
    {
        if (client is null)
            return ServiceResult.Fail("Client invalide.");

        if (string.IsNullOrWhiteSpace(client.Id))
            return ServiceResult.Fail("Id invalide.");

        var existing = await _db.Clients.FirstOrDefaultAsync(c => c.Id == client.Id);
        if (existing is null)
            return ServiceResult.Fail("Client introuvable.");

        var name = (client.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Fail("Nom obligatoire.");

        existing.Name = name;
        existing.Email = (client.Email ?? "").Trim();
        existing.Phone = (client.Phone ?? "").Trim();
        existing.LoyaltyPoints = client.LoyaltyPoints;
        existing.Status = string.IsNullOrWhiteSpace(client.Status) ? existing.Status : client.Status.Trim();
        existing.PersonalizedOffer = (client.PersonalizedOffer ?? "").Trim();

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ServiceResult.Fail("Id invalide.");

        var existing = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
            return ServiceResult.Fail("Client introuvable.");

        _db.Clients.Remove(existing);
        await _db.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}