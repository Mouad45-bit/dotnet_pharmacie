using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using System;
using System.Collections.Generic;

namespace project_pharmacie.Areas.Staff.Pages.Sales
{
	public class HistoryModel : PageModel
	{
		public List<Sale> Sales { get; set; } = new List<Sale>();

		public void OnGet()
		{
			Sales = new List<Sale>
			{
				new Sale
				{
					Id = 101,
					CustomerName = "Thomas Anderson",
					DrugName = "Pilule Rouge",
					Quantity = 2,
					TotalPrice = 15.00m,
					Date = DateTime.Now
				},
				new Sale
				{
					Id = 102,
					CustomerName = "Walter White",
					DrugName = "Sirop Toux",
					Quantity = 1,
					TotalPrice = 24.50m,
					Date = DateTime.Now.AddHours(-2)
				}
			};
		}
	}
}