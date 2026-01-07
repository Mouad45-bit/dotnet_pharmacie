using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Data;
using project_pharmacie.Models;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = new ApplicationUser
        {
            UserName = Input.Login.Trim(),
            Email = $"{Input.Login.Trim()}@local",
            Nom = Input.Nom.Trim(),
            EmailConfirmed = true
        };

        var res = await _userManager.CreateAsync(user, Input.Password);
        if (!res.Succeeded)
        {
            ErrorMessage = string.Join(" ", res.Errors.Select(e => e.Description));
            return Page();
        }

        // IMPORTANT: Register donne toujours PERSONNEL (pas d’auto-admin)
        await _userManager.AddToRoleAsync(user, IdentitySeeder.PersonnelRole);

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToPage("/Dashboard/Index", new { area = "Staff" });
    }

    public class InputModel
    {
        [Required]
        public string Nom { get; set; } = "";

        [Required]
        public string Login { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; } = "";
    }
}