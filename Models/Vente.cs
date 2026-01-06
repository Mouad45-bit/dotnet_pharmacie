using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public class Vente
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime DateVente { get; set; } = DateTime.Now;

    [Required]
    public string ClientId { get; set; } = "";
    public Client? Client { get; set; }

    public ICollection<VenteLigne> Lignes { get; set; } = new List<VenteLigne>();

    public Facture? Facture { get; set; } // 1-1
}

public class VenteLigne
{
    public string VenteId { get; set; } = "";
    public Vente? Vente { get; set; }

    public string ProduitReference { get; set; } = "";
    public Produit? Produit { get; set; }

    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
}