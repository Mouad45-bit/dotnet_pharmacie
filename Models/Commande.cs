using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public class Commande
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime Date { get; set; } = DateTime.Now;
    public double Note { get; set; }
    public decimal PrixTotal { get; set; }

    [Required]
    public string FournisseurId { get; set; } = "";
    public Fournisseur? Fournisseur { get; set; }

    // qui a passé la commande (user Identity)
    public string? PersonnelId { get; set; }
    public ApplicationUser? Personnel { get; set; }

    public ICollection<CommandeLigne> Lignes { get; set; } = new List<CommandeLigne>();
}

public class CommandeLigne
{
    public string CommandeId { get; set; } = "";
    public Commande? Commande { get; set; }

    public string ProduitReference { get; set; } = "";
    public Produit? Produit { get; set; }

    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
}
