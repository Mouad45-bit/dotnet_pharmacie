using System;

namespace project_pharmacie.Models
{
	public class Client
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;

		// Nouveaux champs pour le Secteur C (Fidélité)
		public int LoyaltyPoints { get; set; } = 0;
		public string Status { get; set; } = "Nouveau";   // Ex: Bronze, Silver, Gold
		public string PersonalizedOffer { get; set; } = ""; // Ex: -20% sur Parapharmacie
		public DateTime LastVisit { get; set; } = DateTime.Now;
	}
}