using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Data;
using project_pharmacie.Models;
using project_pharmacie.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PharmacieDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Dev-friendly (tu peux durcir après)
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<PharmacieDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Account/Login";
    opt.AccessDeniedPath = "/Account/AccessDenied";
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(IdentitySeeder.AdminRole));
    options.AddPolicy("Staff", p => p.RequireRole(IdentitySeeder.AdminRole, IdentitySeeder.PersonnelRole));
});

// Razor Pages + protection par zones
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AuthorizeAreaFolder("Staff", "/", "Staff");
    options.Conventions.AllowAnonymousToFolder("/Account");
});

// Services métier
builder.Services.AddScoped<IProduitService, ProduitService>();
builder.Services.AddScoped<ICommandeService, CommandeService>();
builder.Services.AddScoped<IVenteService, VenteService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IFournisseurService, FournisseurService>();

var app = builder.Build();

// migrate + seed (dev)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<PharmacieDbContext>();
    db.Database.Migrate();

    await IdentitySeeder.SeedAsync(services);
    DbSeeder.Seed(db); // ton seed DOMAIN (sans users)
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();