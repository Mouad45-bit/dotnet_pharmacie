using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Models;

namespace project_pharmacie.Data;

public class PharmacieDbContext : IdentityDbContext<ApplicationUser>
{
    public PharmacieDbContext(DbContextOptions<PharmacieDbContext> options) : base(options) { }

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

        // Produit géré par un user Identity (optionnel)
        modelBuilder.Entity<Produit>()
            .HasOne(p => p.Personnel)
            .WithMany()
            .HasForeignKey(p => p.PersonnelId)
            .OnDelete(DeleteBehavior.SetNull);

        // Commande passée par un user Identity (optionnel)
        modelBuilder.Entity<Commande>()
            .HasOne(c => c.Personnel)
            .WithMany()
            .HasForeignKey(c => c.PersonnelId)
            .OnDelete(DeleteBehavior.SetNull);

        // Vente 1-1 Facture
        modelBuilder.Entity<Facture>()
            .HasOne(f => f.Vente)
            .WithOne(v => v.Facture)
            .HasForeignKey<Facture>(f => f.VenteId);

        // VenteLigne (PK composite)
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

        // FournisseurProduit (PK composite)
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

        // CommandeLigne (PK composite)
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

        // Décimals
        modelBuilder.Entity<Produit>().Property(p => p.Prix).HasPrecision(18, 2);
        modelBuilder.Entity<VenteLigne>().Property(p => p.PrixUnitaire).HasPrecision(18, 2);
        modelBuilder.Entity<Commande>().Property(p => p.PrixTotal).HasPrecision(18, 2);
        modelBuilder.Entity<CommandeLigne>().Property(p => p.PrixUnitaire).HasPrecision(18, 2);
    }
}