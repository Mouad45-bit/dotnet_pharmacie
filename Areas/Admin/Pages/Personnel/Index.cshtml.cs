using Microsoft.AspNetCore.Mvc.RazorPages;

namespace project_pharmacie.Areas.Admin.Pages.Personnel;

public class IndexModel : PageModel
{
    public record PersonnelRow(string FullName, string Email, string Role, bool IsActive);

    public List<PersonnelRow> Items { get; private set; } = new();

    public void OnGet()
    {
        // Mock data juste pour tester l’Area + Tailwind
        Items = new List<PersonnelRow>
        {
            new("Admin Demo", "admin@pharmacie.local", "Admin", true),
            new("Sara Pharmacien", "sara@pharmacie.local", "Pharmacien", true),
            new("Yassine Caissier", "yassine@pharmacie.local", "Caissier", false),
        };
    }
}
