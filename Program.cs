using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURATION DES SERVICES (Le "Builder")
// ==========================================

// A. Connexion à la base de données
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));

// B. Configuration de Identity (Utilisateurs + Rôles)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	// 👇 VOS RÈGLES DE MOT DE PASSE SIMPLIFIÉES
	// Avec ça, le mot de passe "1234" ou "pass" fonctionnera !
	options.Password.RequireDigit = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequiredLength = 4;

	// Optionnel : Confirmer l'email n'est pas requis pour se connecter
	options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// C. Configuration des pages Razor avec SÉCURITÉ GLOBALE
builder.Services.AddRazorPages(options =>
{
	// On verrouille tout le dossier Admin (il faut être connecté)
	options.Conventions.AuthorizeAreaFolder("Admin", "/");
	// On verrouille tout le dossier Staff (il faut être connecté)
	options.Conventions.AuthorizeAreaFolder("Staff", "/");
});

var app = builder.Build();

// ==========================================
// 2. CONFIGURATION DU PIPELINE HTTP (L'Application)
// ==========================================

// Gestion des erreurs en production
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// D. Activer l'authentification (Ordre très important !)
app.UseAuthentication(); // 1. Qui est-ce ?
app.UseAuthorization();  // 2. A-t-il le droit ?

app.MapRazorPages();

// ==========================================
// 3. SEEDING (Création auto des données)
// ==========================================
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		// On attend que la création soit finie avec 'await'
		await DbInitializer.SeedRolesAndUsersAsync(services);
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "Erreur lors du Seed (Rôles/Users).");
	}
}

// ==========================================
// 4. DÉMARRAGE
// ==========================================
app.Run();