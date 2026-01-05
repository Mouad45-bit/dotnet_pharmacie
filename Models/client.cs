using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public class Client
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required, MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(180)]
    public string Email { get; set; } = "";

    [MaxLength(40)]
    public string Phone { get; set; } = "";

    public int LoyaltyPoints { get; set; } = 0;

    [MaxLength(40)]
    public string Status { get; set; } = "Nouveau";

    [MaxLength(300)]
    public string PersonalizedOffer { get; set; } = "";

    public ICollection<Vente> Ventes { get; set; } = new List<Vente>();
}