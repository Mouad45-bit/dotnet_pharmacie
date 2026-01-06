using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public class Produit
{
    [Key, MaxLength(50)]
    public string Reference { get; set; } = "";

    [Required, MaxLength(150)]
    public string Nom { get; set; } = "";

    public decimal Prix { get; set; }
    public int Quantite { get; set; }
    public DateTime? DatePeremption { get; set; }

    // géré par un user Identity
    public string? PersonnelId { get; set; }
    public ApplicationUser? Personnel { get; set; }

    public ICollection<VenteLigne> VenteLignes { get; set; } = new List<VenteLigne>();
    public ICollection<CommandeLigne> CommandeLignes { get; set; } = new List<CommandeLigne>();

    public ICollection<FournisseurProduit> Fournisseurs { get; set; } = new List<FournisseurProduit>();
}