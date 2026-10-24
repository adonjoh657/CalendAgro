using SQLite;

namespace CalendAgro.Data;

// Ligne SQLite d'un conseil court (12 mots maximum) d'une phase d'une culture.
[Table("Conseils")]
public sealed class ConseilRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    [Indexed]
    public string CultureId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    // Valeur de TypeEtape.
    public int Phase { get; set; }

    public string Titre { get; set; } = string.Empty;

    public string Texte { get; set; } = string.Empty;

    public int Ordre { get; set; }

    public long UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
