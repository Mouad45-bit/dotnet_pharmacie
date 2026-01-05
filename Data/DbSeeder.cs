using project_pharmacie.Models;

namespace project_pharmacie.Data;

public static class DbSeeder
{
    public static void Seed(PharmacieDbContext db)
    {
        // Si déjà seedé, on ne refait pas (évite doublons)
        if (db.Clients.Any() || db.Produits.Any() || db.Fournisseurs.Any() || db.Utilisateurs.Any())
            return;

        var rng = new Random(2026);

        // -----------------------------
        // 1) Utilisateurs (1 admin + 8 personnels)
        // -----------------------------
        var admin = new Administrateur
        {
            Nom = "Admin Principal",
            Login = "admin",
            PasswordHash = "hash",
            Role = "ADMIN"
        };

        var personnels = new List<Personnel>();
        for (int i = 1; i <= 8; i++)
        {
            personnels.Add(new Personnel
            {
                Nom = $"Staff {i}",
                Login = $"staff{i}",
                PasswordHash = "hash",
                Role = "PERSONNEL",
                Administrateur = admin
            });
        }

        db.Utilisateurs.Add(admin);
        db.Utilisateurs.AddRange(personnels);

        // -----------------------------
        // 2) Clients (50)
        // -----------------------------
        string[] prenoms = { "Omar", "Sara", "Yassine", "Imane", "Hajar", "Karim", "Aya", "Rania", "Anas", "Salma", "Khalid", "Nadia" };
        string[] noms = { "El Amrani", "Bennani", "Lahlou", "Alaoui", "Fassi", "Cherkaoui", "Idrissi", "Belkadi", "Rifai", "Haddad" };

        var clients = new List<Client>();
        for (int i = 1; i <= 50; i++)
        {
            var fullName = $"{prenoms[rng.Next(prenoms.Length)]} {noms[rng.Next(noms.Length)]}";
            var points = rng.Next(0, 220);

            clients.Add(new Client
            {
                Name = fullName,
                Email = $"client{i}@mail.com",
                Phone = $"06{rng.Next(10000000, 99999999)}",
                LoyaltyPoints = points,
                Status = points >= 120 ? "Or" : points >= 60 ? "Argent" : "Nouveau",
                PersonalizedOffer = points >= 120 ? "-15% Skincare" : points >= 60 ? "-10% Vitamines" : ""
            });
        }

        db.Clients.AddRange(clients);

        // -----------------------------
        // 3) Fournisseurs (12)
        // -----------------------------
        var fournisseurs = new List<Fournisseur>
        {
            new Fournisseur { Nom = "PharmaSupply", NoteGlobale = 4.5 },
            new Fournisseur { Nom = "MediPlus", NoteGlobale = 4.1 },
            new Fournisseur { Nom = "BioCare Distribution", NoteGlobale = 4.7 },
            new Fournisseur { Nom = "Atlas Pharma", NoteGlobale = 3.9 },
            new Fournisseur { Nom = "SantéExpress", NoteGlobale = 4.2 },
            new Fournisseur { Nom = "WellnessPro", NoteGlobale = 4.0 },
            new Fournisseur { Nom = "ParapharmX", NoteGlobale = 4.6 },
            new Fournisseur { Nom = "DermaSource", NoteGlobale = 4.3 },
            new Fournisseur { Nom = "NutriHub", NoteGlobale = 4.4 },
            new Fournisseur { Nom = "CareLog", NoteGlobale = 3.8 },
            new Fournisseur { Nom = "Hygia Partners", NoteGlobale = 4.1 },
            new Fournisseur { Nom = "VitaDistrib", NoteGlobale = 4.5 }
        };

        db.Fournisseurs.AddRange(fournisseurs);

        // -----------------------------
        // 4) Produits (120) + liaison staff qui gère
        // -----------------------------
        var catalog = new (string Nom, decimal PrixMin, decimal PrixMax)[]
        {
            ("Doliprane 1g", 10, 18),
            ("Paracetamol 500mg", 6, 12),
            ("Vitamine C", 18, 40),
            ("Magnésium", 25, 60),
            ("Crème hydratante", 40, 120),
            ("Sérum visage", 90, 250),
            ("Gel douche", 20, 55),
            ("Shampooing anti-chute", 55, 140),
            ("Crème solaire SPF50", 70, 180),
            ("Spray nasal", 25, 60),
            ("Pastilles gorge", 15, 45),
            ("Lait bébé", 80, 200),
            ("Thermomètre digital", 35, 120),
            ("Désinfectant", 20, 70),
            ("Complément Omega 3", 60, 180),
        };

        var produits = new List<Produit>();
        for (int i = 1; i <= 120; i++)
        {
            var item = catalog[rng.Next(catalog.Length)];
            var prix = Math.Round((decimal)(item.PrixMin + rng.NextDouble() * (double)(item.PrixMax - item.PrixMin)), 2);

            var stock = rng.Next(0, 160);
            var refProd = $"P-{i:D4}";

            // un staff “gère” le produit
            var staff = personnels[rng.Next(personnels.Count)];

            // quelques dates de péremption
            DateTime? peremption = null;
            if (rng.NextDouble() < 0.35)
                peremption = DateTime.Today.AddDays(rng.Next(30, 900));

            produits.Add(new Produit
            {
                Reference = refProd,
                Nom = item.Nom,
                Prix = prix,
                Quantite = stock,
                DatePeremption = peremption,
                Personnel = staff
            });
        }

        db.Produits.AddRange(produits);

        // important : SaveChanges ici pour obtenir les IDs et stabiliser les relations
        db.SaveChanges();

        // -----------------------------
        // 5) FournisseurProduits (chaque fournisseur fournit 25 à 60 produits)
        // -----------------------------
        foreach (var f in fournisseurs)
        {
            var count = rng.Next(25, 61);
            var subset = produits.OrderBy(_ => rng.Next()).Take(count).ToList();

            foreach (var p in subset)
            {
                db.FournisseurProduits.Add(new FournisseurProduit
                {
                    FournisseurId = f.Id,
                    ProduitReference = p.Reference
                });
            }
        }

        db.SaveChanges();

        // -----------------------------
        // 6) Commandes (40) + lignes (3 à 10)
        // -----------------------------
        var commandes = new List<Commande>();

        for (int i = 1; i <= 40; i++)
        {
            var f = fournisseurs[rng.Next(fournisseurs.Count)];
            var staff = personnels[rng.Next(personnels.Count)];

            var cmd = new Commande
            {
                Date = DateTime.Today.AddDays(-rng.Next(0, 90)),
                Note = Math.Round(3.5 + rng.NextDouble() * 1.5, 1), // 3.5 -> 5.0
                FournisseurId = f.Id,
                PersonnelId = staff.Id
            };

            var lignesCount = rng.Next(3, 11);

            // choisir parmi les produits fournis par ce fournisseur
            var prodRefs = db.FournisseurProduits
                .Where(x => x.FournisseurId == f.Id)
                .Select(x => x.ProduitReference)
                .ToList();

            // fallback si vide
            if (prodRefs.Count == 0)
                prodRefs = produits.Select(p => p.Reference).ToList();

            var chosenRefs = prodRefs.OrderBy(_ => rng.Next()).Take(lignesCount).ToList();
            decimal total = 0m;

            foreach (var pr in chosenRefs)
            {
                var prod = produits.First(p => p.Reference == pr);
                var qte = rng.Next(5, 50);
                var prixU = prod.Prix * (decimal)(0.65 + rng.NextDouble() * 0.25); // prix fournisseur ~ 65-90%
                prixU = Math.Round(prixU, 2);

                cmd.Lignes.Add(new CommandeLigne
                {
                    ProduitReference = prod.Reference,
                    Quantite = qte,
                    PrixUnitaire = prixU
                });

                total += prixU * qte;

                // augmenter le stock (réception “supposée”)
                prod.Quantite += qte;
            }

            cmd.PrixTotal = Math.Round(total, 2);
            commandes.Add(cmd);
        }

        db.Commandes.AddRange(commandes);
        db.SaveChanges();

        // -----------------------------
        // 7) Ventes (80) + lignes (1 à 6) + facture 1-1
        // -----------------------------
        var ventes = new List<Vente>();

        for (int i = 1; i <= 80; i++)
        {
            var client = clients[rng.Next(clients.Count)];
            var vente = new Vente
            {
                DateVente = DateTime.Today.AddDays(-rng.Next(0, 60)).AddMinutes(rng.Next(0, 600)),
                ClientId = client.Id
            };

            var lignesCount = rng.Next(1, 7);
            var chosen = produits
                .Where(p => p.Quantite > 0)
                .OrderBy(_ => rng.Next())
                .Take(lignesCount)
                .ToList();

            decimal total = 0m;

            foreach (var prod in chosen)
            {
                var qte = rng.Next(1, 5);
                if (prod.Quantite < qte) qte = Math.Max(1, prod.Quantite);

                var prixU = prod.Prix;
                vente.Lignes.Add(new VenteLigne
                {
                    ProduitReference = prod.Reference,
                    Quantite = qte,
                    PrixUnitaire = prixU
                });

                total += prixU * qte;

                // diminution stock
                prod.Quantite -= qte;

                // points fidélité
                client.LoyaltyPoints += (int)Math.Floor((double)(prixU * qte) / 20.0);
            }

            // statut client mis à jour
            client.Status = client.LoyaltyPoints >= 120 ? "Or" : client.LoyaltyPoints >= 60 ? "Argent" : "Nouveau";

            // facture 1-1
            vente.Facture = new Facture
            {
                Sujet = $"Facture Vente #{i:D4}",
                DateFacture = vente.DateVente
            };

            ventes.Add(vente);
        }

        db.Ventes.AddRange(ventes);
        db.SaveChanges();
    }
}