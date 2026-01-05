using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public class Fournisseur
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [Required, MaxLength(150)]
    public string Nom { get; set; } = "";

    public double NoteGlobale { get; set; }

    public ICollection<Commande> Commandes { get; set; } = new List<Commande>();
    public ICollection<FournisseurProduit> Produits { get; set; } = new List<FournisseurProduit>();
}

public class FournisseurProduit
{
    public string FournisseurId { get; set; } = "";
    public Fournisseur? Fournisseur { get; set; }

    public string ProduitReference { get; set; } = "";
    public Produit? Produit { get; set; }
}