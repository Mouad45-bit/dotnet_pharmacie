using project_pharmacie.Models;

namespace project_pharmacie.Services;

public interface IClientService
{
    Task<(List<Client> items, int total)> SearchAsync(string? q);
    Task<ServiceResult> DeleteAsync(string id);
}