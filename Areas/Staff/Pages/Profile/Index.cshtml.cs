using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;

namespace project_pharmacie.Areas.Staff.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly PharmacieDbContext _db;

        public IndexModel(PharmacieDbContext db) => _db = db;

        public bool Found { get; private set; }

        public string Username { get; private set; } = "";
        public string Role { get; private set; } = "";
        public string Email { get; private set; } = "";      // ici on affiche Login (car pas de champ Email)
        public string EmployeeId { get; private set; } = "";  // ici on affiche l'Id DB

        [BindProperty]
        public string? CurrentPassword { get; set; }

        [BindProperty]
        public string? NewPassword { get; set; }

        [TempData]
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            await LoadUserFromDbAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Tant qu'on n'a pas d'auth réelle (session/Identity),
            // on ne change PAS le mot de passe en DB.
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                Message = "Modification du mot de passe désactivée tant que l’authentification n’est pas en place.";
            }

            await LoadUserFromDbAsync();
            return Page();
        }

        private async Task LoadUserFromDbAsync()
        {
            // 1) Essayer Personnel
            var p = await _db.Personnels
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .FirstOrDefaultAsync();

            if (p is not null)
            {
                Found = true;
                Username = p.Nom;
                Role = p.Role;
                Email = p.Login;      // on garde "Email" côté UI pour éviter de modifier le cshtml
                EmployeeId = p.Id;
                return;
            }

            // 2) Sinon essayer Administrateur
            var a = await _db.Administrateurs
                .AsNoTracking()
                .OrderBy(x => x.Nom)
                .FirstOrDefaultAsync();

            if (a is not null)
            {
                Found = true;
                Username = a.Nom;
                Role = a.Role;
                Email = a.Login;
                EmployeeId = a.Id;
                return;
            }

            // 3) Aucun user trouvé
            Found = false;
            Username = "";
            Role = "";
            Email = "";
            EmployeeId = "";
        }
    }
}