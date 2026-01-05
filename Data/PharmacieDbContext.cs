using Microsoft.EntityFrameworkCore;
using project_pharmacie.Models;

namespace project_pharmacie.Data;

public class PharmacieDbContext : DbContext
{
    public PharmacieDbContext(DbContextOptions<PharmacieDbContext> options) : base(options) { }

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Administrateur> Administrateurs => Set<Administrateur>();
    public DbSet<Personnel> Personnels => Set<Personnel>();

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Produit> Produits => Set<Produit>();

    public DbSet<Vente> Ventes => Set<Vente>();
    public DbSet<VenteLigne> VenteLignes => Set<VenteLigne>();
    public DbSet<Facture> Factures => Set<Facture>();

    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();
    public DbSet<FournisseurProduit> FournisseurProduits => Set<FournisseurProduit>();

    public DbSet<Commande> Commandes => Set<Commande>();
    public DbSet<CommandeLigne> CommandeLignes => Set<CommandeLigne>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Utilisateur XOR (TPH)
        modelBuilder.Entity<Utilisateur>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Administrateur>("ADMIN")
            .HasValue<Personnel>("PERSONNEL");

        modelBuilder.Entity<Utilisateur>()
            .HasIndex(u => u.Login)
            .IsUnique();

        // Admin 1..* Personnel
        modelBuilder.Entity<Personnel>()
            .HasOne(p => p.Administrateur)
            .WithMany(a => a.Personnels)
            .HasForeignKey(p => p.AdministrateurId)
            .OnDelete(DeleteBehavior.SetNull);

        // Personnel 1..* Produit
        modelBuilder.Entity<Produit>()
            .HasOne(p => p.Personnel)
            .WithMany(s => s.ProduitsGeres)
            .HasForeignKey(p => p.PersonnelId)
            .OnDelete(DeleteBehavior.SetNull);

        // Client 1..* Vente
        modelBuilder.Entity<Vente>()
            .HasOne(v => v.Client)
            .WithMany(c => c.Ventes)
            .HasForeignKey(v => v.ClientId);

        // Vente 1-1 Facture
        modelBuilder.Entity<Facture>()
            .HasOne(f => f.Vente)
            .WithOne(v => v.Facture)
            .HasForeignKey<Facture>(f => f.VenteId);

        // VenteLigne (Vente N..N Produit + quantite/prix)
        modelBuilder.Entity<VenteLigne>()
            .HasKey(x => new { x.VenteId, x.ProduitReference });

        modelBuilder.Entity<VenteLigne>()
            .HasOne(x => x.Vente)
            .WithMany(v => v.Lignes)
            .HasForeignKey(x => x.VenteId);

        modelBuilder.Entity<VenteLigne>()
            .HasOne(x => x.Produit)
            .WithMany(p => p.VenteLignes)
            .HasForeignKey(x => x.ProduitReference);

        // FournisseurProduit (Fournisseur N..N Produit)
        modelBuilder.Entity<FournisseurProduit>()
            .HasKey(x => new { x.FournisseurId, x.ProduitReference });

        modelBuilder.Entity<FournisseurProduit>()
            .HasOne(x => x.Fournisseur)
            .WithMany(f => f.Produits)
            .HasForeignKey(x => x.FournisseurId);

        modelBuilder.Entity<FournisseurProduit>()
            .HasOne(x => x.Produit)
            .WithMany(p => p.Fournisseurs)
            .HasForeignKey(x => x.ProduitReference);

        // Fournisseur 1..* Commande
        modelBuilder.Entity<Commande>()
            .HasOne(c => c.Fournisseur)
            .WithMany(f => f.Commandes)
            .HasForeignKey(c => c.FournisseurId);

        // Personnel 1..* Commande (passer commande)
        modelBuilder.Entity<Commande>()
            .HasOne(c => c.Personnel)
            .WithMany(p => p.CommandesPassees)
            .HasForeignKey(c => c.PersonnelId)
            .OnDelete(DeleteBehavior.SetNull);

        // CommandeLigne (Commande N..N Produit + quantite/prix)
        modelBuilder.Entity<CommandeLigne>()
            .HasKey(x => new { x.CommandeId, x.ProduitReference });

        modelBuilder.Entity<CommandeLigne>()
            .HasOne(x => x.Commande)
            .WithMany(c => c.Lignes)
            .HasForeignKey(x => x.CommandeId);

        modelBuilder.Entity<CommandeLigne>()
            .HasOne(x => x.Produit)
            .WithMany(p => p.CommandeLignes)
            .HasForeignKey(x => x.ProduitReference);

        // Décimals (SQLite: important)
        modelBuilder.Entity<Produit>().Property(p => p.Prix).HasPrecision(18, 2);
        modelBuilder.Entity<VenteLigne>().Property(p => p.PrixUnitaire).HasPrecision(18, 2);
        modelBuilder.Entity<Commande>().Property(p => p.PrixTotal).HasPrecision(18, 2);
        modelBuilder.Entity<CommandeLigne>().Property(p => p.PrixUnitaire).HasPrecision(18, 2);
    }
}