using SQLite;

namespace CalendAgro.Data;

// Saisons d'une région, dans l'ordre d'affichage du sélecteur.
[Table("Saisons")]
public sealed class SaisonRow
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Zone { get; set; } = string.Empty;

    public string Nom { get; set; } = string.Empty;

    public int ZoneOrdre { get; set; }

    public int Ordre { get; set; }
}
