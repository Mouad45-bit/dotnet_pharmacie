namespace project_pharmacie.Areas.Admin.ViewModels;

public enum PersonnelPoste
{
    Pharmacien,
    Caissier,
    Preparateur
}

public class PersonnelForm
{
    public string Nom { get; set; } = "";
    public string Login { get; set; } = "";
}