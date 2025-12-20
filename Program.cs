var builder = WebApplication.CreateBuilder(args);

// Ajouter les services Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configuration du pipeline HTTP
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();

// ✅ LA LIGNE CLÉ : Autorise l'accès au dossier wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ✅ VERSION SIMPLE : On mappe les pages sans "StaticAssets" complexes
app.MapRazorPages();

app.Run();