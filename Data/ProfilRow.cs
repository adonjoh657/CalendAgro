using SQLite;

namespace CalendAgro.Data;

// Ligne SQLite du profil local. Aucun mot de passe n'est stocké ici :
// seulement l'identité affichée et la langue choisie.
[Table("Profil")]
public sealed class ProfilRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Nom { get; set; } = string.Empty;

    public string Identifiant { get; set; } = string.Empty;

    public string Telephone { get; set; } = string.Empty;

    public string Langue { get; set; } = string.Empty;

    // Colonnes de synchronisation (AGENTS §6 bis), remplies dès l'écriture.
    public string UserId { get; set; } = string.Empty;

    public long UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
