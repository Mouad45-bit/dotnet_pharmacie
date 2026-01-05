using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public abstract class Utilisateur
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required, MaxLength(120)]
    public string Nom { get; set; } = "";

    [Required, MaxLength(80)]
    public string Login { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required, MaxLength(30)]
    public string Role { get; set; } = ""; // "ADMIN" ou "PERSONNEL"
}

public class Administrateur : Utilisateur
{
    public ICollection<Personnel> Personnels { get; set; } = new List<Personnel>();
}

public class Personnel : Utilisateur
{
    public string? AdministrateurId { get; set; }
    public Administrateur? Administrateur { get; set; }

    public ICollection<Produit> ProduitsGeres { get; set; } = new List<Produit>();
    public ICollection<Commande> CommandesPassees { get; set; } = new List<Commande>();
}
