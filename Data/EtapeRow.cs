using SQLite;

namespace CalendAgro.Data;

// Ligne SQLite d'une étape : période d'une phase pour une culture, une zone et une saison.
[Table("Etapes")]
public sealed class EtapeRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string CultureId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    // Valeur de TypeEtape.
    public int Type { get; set; }

    public string Zone { get; set; } = string.Empty;

    public string Saison { get; set; } = string.Empty;

    // Mois de 1 (janvier) à 12 (décembre), bornes incluses.
    public int MoisDebut { get; set; }

    public int MoisFin { get; set; }

    public long UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
