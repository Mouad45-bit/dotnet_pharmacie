namespace project_pharmacie.Areas.Admin.Services;

public enum PersonnelRole
{
    Pharmacien,
    Caissier,
    Preparateur,
    Admin
}

public class Personnel
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public PersonnelRole Role { get; set; } = PersonnelRole.Pharmacien;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PersonnelForm
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public PersonnelRole Role { get; set; } = PersonnelRole.Pharmacien;
    public bool IsActive { get; set; } = true;
}
