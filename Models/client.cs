using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public class Client
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required, MaxLength(120)]
    public string Nom { get; set; } = "";

    public ICollection<Vente> Ventes { get; set; } = new List<Vente>();
}