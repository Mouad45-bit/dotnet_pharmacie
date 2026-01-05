using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public class Facture
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required]
    public string VenteId { get; set; } = "";
    public Vente? Vente { get; set; }

    [MaxLength(200)]
    public string Sujet { get; set; } = "";

    public DateTime DateFacture { get; set; } = DateTime.Now;
}