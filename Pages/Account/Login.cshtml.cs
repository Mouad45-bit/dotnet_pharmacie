using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager)
        => _signInManager = signInManager;

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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return Redirect("/Staff");
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