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
}