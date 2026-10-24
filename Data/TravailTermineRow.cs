using SQLite;

namespace CalendAgro.Data;

// Ligne SQLite d'une phase terminée : la date de semis ne suffit pas à savoir
// quels travaux sont faits, on garde donc une trace par parcelle et par phase.
[Table("TravauxTermines")]
public sealed class TravailTermineRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string ParcelleId { get; set; } = string.Empty;

    public int Phase { get; set; }

    // Date de réalisation au format AAAA-MM-JJ.
    public string DateTerminee { get; set; } = string.Empty;

    // Colonnes de synchronisation (AGENTS §6 bis), remplies dès l'écriture.
    public string UserId { get; set; } = string.Empty;

    public long UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
