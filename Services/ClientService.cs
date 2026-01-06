using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public class ClientService
{
    private readonly PharmacieDbContext _db;

    public ClientService(PharmacieDbContext db)
    {
        _db = db;
    }

    public async Task<List<Client>> GetAllAsync()
    {
        return await _db.Clients
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Client?> GetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        return await _db.Clients
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // UNE SEULE méthode CreateAsync (c’est ça qui corrige CS0111)
    public async Task<ServiceResult<Client>> CreateAsync(Client input)
    {
        if (input is null)
            return ServiceResult<Client>.Fail("Client invalide.");

        var name = (input.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult<Client>.Fail("Nom obligatoire.");

        var email = (input.Email ?? "").Trim();
        var phone = (input.Phone ?? "").Trim();

        var entity = new Client
        {
            Name = name,
            Email = email,
            Phone = phone,
            LoyaltyPoints = input.LoyaltyPoints,
            Status = string.IsNullOrWhiteSpace(input.Status) ? "Nouveau" : input.Status.Trim(),
            PersonalizedOffer = (input.PersonalizedOffer ?? "").Trim()
        };

        _db.Clients.Add(entity);
        await _db.SaveChangesAsync();

        return ServiceResult<Client>.Ok(entity);
    }

    public async Task<ServiceResult> UpdateAsync(string id, Client input)
    {
        if (string.IsNullOrWhiteSpace(id))
            return ServiceResult.Fail("Id invalide.");

        var existing = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
            return ServiceResult.Fail("Client introuvable.");

        var name = (input.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult.Fail("Nom obligatoire.");

        existing.Name = name;
        existing.Email = (input.Email ?? "").Trim();
        existing.Phone = (input.Phone ?? "").Trim();
        existing.LoyaltyPoints = input.LoyaltyPoints;
        existing.Status = string.IsNullOrWhiteSpace(input.Status) ? existing.Status : input.Status.Trim();
        existing.PersonalizedOffer = (input.PersonalizedOffer ?? "").Trim();

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