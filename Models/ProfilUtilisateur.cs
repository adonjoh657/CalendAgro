namespace CalendAgro.Models;

// Profil local : identité affichée dans l'application, sans mot de passe.
public sealed record ProfilUtilisateur(string Nom, string Identifiant, string Telephone);
