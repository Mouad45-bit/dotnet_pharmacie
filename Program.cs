// Program.cs

using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

builder.Services.AddDbContext<PharmacieDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProduitService, ProduitService>();
builder.Services.AddScoped<ICommandeService, CommandeService>();
builder.Services.AddScoped<IVenteService, VenteService>();
builder.Services.AddScoped<IPersonnelService, PersonnelService>();

var app = builder.Build();

// Auto-migrate au démarrage (utile pour éviter "table not found")
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PharmacieDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// autorise l'accès à wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();