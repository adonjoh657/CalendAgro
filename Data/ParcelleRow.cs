using SQLite;

namespace CalendAgro.Data;

// Ligne SQLite d'une parcelle. Les colonnes de synchronisation (UserId,
// UpdatedAt, IsDeleted) sont prévues par AGENTS §6 bis et inutilisées pour l'instant.
[Table("Parcelles")]
public sealed class ParcelleRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Nom { get; set; } = string.Empty;

    public string CultureId { get; set; } = string.Empty;

    public string CultureNom { get; set; } = string.Empty;

    public double SurfaceHa { get; set; }

    public string Region { get; set; } = string.Empty;

    // Date de semis au format AAAA-MM-JJ (DateOnly n'est pas géré par sqlite-net).
    public string DateSemis { get; set; } = string.Empty;

    // Date de dernière modification en ticks UTC.
    public long UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
