using System;

namespace project_pharmacie.Models
{
	public class Sales
	{
		public int Id { get; set; }
		public string CustomerName { get; set; } = string.Empty; // Nom du client
		public string DrugName { get; set; } = string.Empty;     // Nom du médicament
		public int Quantity { get; set; }                        // Quantité
		public decimal TotalPrice { get; set; }                  // Prix total
		public DateTime Date { get; set; } = DateTime.Now;       // Date de vente
        public string Status { get; set; } = "PAID";
    }
}