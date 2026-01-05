using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_pharmacie.Migrations
{
    /// <inheritdoc />
    public partial class InitDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    LoyaltyPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    PersonalizedOffer = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NoteGlobale = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Login = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    UserType = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    AdministrateurId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Utilisateurs_AdministrateurId",
                        column: x => x.AdministrateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ventes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DateVente = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClientId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commandes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Note = table.Column<double>(type: "REAL", nullable: false),
                    PrixTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FournisseurId = table.Column<string>(type: "TEXT", nullable: false),
                    PersonnelId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commandes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commandes_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Commandes_Utilisateurs_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    Reference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Prix = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Quantite = table.Column<int>(type: "INTEGER", nullable: false),
                    DatePeremption = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PersonnelId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => x.Reference);
                    table.ForeignKey(
                        name: "FK_Produits_Utilisateurs_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Factures",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    VenteId = table.Column<string>(type: "TEXT", nullable: false),
                    Sujet = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DateFacture = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Factures_Ventes_VenteId",
                        column: x => x.VenteId,
                        principalTable: "Ventes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommandeLignes",
                columns: table => new
                {
                    CommandeId = table.Column<string>(type: "TEXT", nullable: false),
                    ProduitReference = table.Column<string>(type: "TEXT", nullable: false),
                    Quantite = table.Column<int>(type: "INTEGER", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandeLignes", x => new { x.CommandeId, x.ProduitReference });
                    table.ForeignKey(
                        name: "FK_CommandeLignes_Commandes_CommandeId",
                        column: x => x.CommandeId,
                        principalTable: "Commandes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommandeLignes_Produits_ProduitReference",
                        column: x => x.ProduitReference,
                        principalTable: "Produits",
                        principalColumn: "Reference",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FournisseurProduits",
                columns: table => new
                {
                    FournisseurId = table.Column<string>(type: "TEXT", nullable: false),
                    ProduitReference = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FournisseurProduits", x => new { x.FournisseurId, x.ProduitReference });
                    table.ForeignKey(
                        name: "FK_FournisseurProduits_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FournisseurProduits_Produits_ProduitReference",
                        column: x => x.ProduitReference,
                        principalTable: "Produits",
                        principalColumn: "Reference",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenteLignes",
                columns: table => new
                {
                    VenteId = table.Column<string>(type: "TEXT", nullable: false),
                    ProduitReference = table.Column<string>(type: "TEXT", nullable: false),
                    Quantite = table.Column<int>(type: "INTEGER", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenteLignes", x => new { x.VenteId, x.ProduitReference });
                    table.ForeignKey(
                        name: "FK_VenteLignes_Produits_ProduitReference",
                        column: x => x.ProduitReference,
                        principalTable: "Produits",
                        principalColumn: "Reference",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VenteLignes_Ventes_VenteId",
                        column: x => x.VenteId,
                        principalTable: "Ventes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommandeLignes_ProduitReference",
                table: "CommandeLignes",
                column: "ProduitReference");

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_FournisseurId",
                table: "Commandes",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_PersonnelId",
                table: "Commandes",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_VenteId",
                table: "Factures",
                column: "VenteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FournisseurProduits_ProduitReference",
                table: "FournisseurProduits",
                column: "ProduitReference");

            migrationBuilder.CreateIndex(
                name: "IX_Produits_PersonnelId",
                table: "Produits",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_AdministrateurId",
                table: "Utilisateurs",
                column: "AdministrateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Login",
                table: "Utilisateurs",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VenteLignes_ProduitReference",
                table: "VenteLignes",
                column: "ProduitReference");

            migrationBuilder.CreateIndex(
                name: "IX_Ventes_ClientId",
                table: "Ventes",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandeLignes");

            migrationBuilder.DropTable(
                name: "Factures");

            migrationBuilder.DropTable(
                name: "FournisseurProduits");

            migrationBuilder.DropTable(
                name: "VenteLignes");

            migrationBuilder.DropTable(
                name: "Commandes");

            migrationBuilder.DropTable(
                name: "Produits");

            migrationBuilder.DropTable(
                name: "Ventes");

            migrationBuilder.DropTable(
                name: "Fournisseurs");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
