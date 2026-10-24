using SQLite;

namespace CalendAgro.Data;

// Ligne SQLite d'un rappel : seul l'état actif est stocké, les dates sont
// recalculées depuis les étapes de référence et la parcelle.
[Table("Rappels")]
public sealed class RappelRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string ParcelleId { get; set; } = string.Empty;

    public int Phase { get; set; }

    public bool Actif { get; set; }

    // Colonnes de synchronisation (AGENTS §6 bis), remplies dès l'écriture.
    public string UserId { get; set; } = string.Empty;

    public long UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
