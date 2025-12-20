using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using System.Collections.Generic;

namespace project_pharmacie.Areas.Staff.Pages.Clients
{
	public class IndexModel : PageModel
	{
		public List<Client> Clients { get; set; } = new List<Client>();

		public void OnGet()
		{
			// Simulation des données (Mock)
			Clients = new List<Client>
			{
				new Client
				{
					Id = 1,
					Name = "Jean Dupont",
					Email = "jean.dupont@email.com",
					LoyaltyPoints = 120,
					Status = "Or",
					PersonalizedOffer = "-10% Gamme Bio"
				},
				new Client
				{
					Id = 2,
					Name = "Marie Curie",
					Email = "marie.curie@science.org",
					LoyaltyPoints = 45,
					Status = "Argent",
					PersonalizedOffer = ""
				},
				new Client
				{
					Id = 3,
					Name = "Paul Atreides",
					Email = "paul@dune.com",
					LoyaltyPoints = 5,
					Status = "Nouveau",
					PersonalizedOffer = ""
				}
			};
		}
	}
}