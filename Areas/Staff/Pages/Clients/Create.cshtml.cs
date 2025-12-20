using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Clients
{
	public class CreateModel : PageModel
	{
		[BindProperty]
		public Client Client { get; set; } = new Client();

		public IActionResult OnPost()
		{
			if (!ModelState.IsValid) return Page();

			// TODO: Sauvegarder en BDD ici

			TempData["Message"] = $"Client {Client.Name} créé avec succès.";
			return RedirectToPage("./Index");
		}
	}
}