using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(120)]
    public string Nom { get; set; } = "";
}