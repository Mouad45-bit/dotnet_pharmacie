using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        var res = await _signInManager.PasswordSignInAsync(
            Input.Login, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        if (!res.Succeeded)
        {
            ErrorMessage = "Login ou mot de passe incorrect.";
            return Page();
        }

        // 1) Si on a un returnUrl valide, on le respecte
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        // 2) Sinon on redirige selon le rôle
        var user =
            await _userManager.FindByNameAsync(Input.Login)
            ?? await _userManager.FindByEmailAsync(Input.Login);

        if (user is not null && await _userManager.IsInRoleAsync(user, "Admin"))
            return RedirectToPage("/Personnel/Index", new { area = "Admin" });

        return RedirectToPage("/Dashboard/Index", new { area = "Staff" });
    }

    public class InputModel
    {
        [Required]
        public string Login { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}