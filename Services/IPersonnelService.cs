using project_pharmacie.Areas.Admin.ViewModels;
using project_pharmacie.Models;

namespace project_pharmacie.Services;

public interface IPersonnelService
{
    Task<List<Personnel>> SearchAsync(string? q);
    Task<Personnel?> GetAsync(string id);

    Task<ServiceResult<Personnel>> CreateAsync(PersonnelForm form);
    Task<ServiceResult> UpdateAsync(string id, PersonnelForm form);
    Task<ServiceResult> DeleteAsync(string id);
}