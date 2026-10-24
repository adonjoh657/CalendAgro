using SQLite;

namespace CalendAgro.Data;

// Ligne SQLite d'un rappel personnel. Date et heure sont stockées en texte
// invariant pour rester lisibles et stables entre les sessions.
[Table("RappelsPersonnels")]
public sealed class RappelPersonnelRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Titre { get; set; } = string.Empty;

    public string DateDebut { get; set; } = string.Empty;

    public string Heure { get; set; } = string.Empty;

    public int Frequence { get; set; }

    public bool Actif { get; set; } = true;

    // Colonnes de synchronisation (AGENTS §6 bis), remplies dès l'écriture.
    public string UserId { get; set; } = string.Empty;

    public long UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
