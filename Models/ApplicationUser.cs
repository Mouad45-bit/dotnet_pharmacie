using Microsoft.AspNetCore.Identity;

namespace project_pharmacie.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string NomComplet { get; set; }
		public bool EstActif { get; set; } = true;
	}
}