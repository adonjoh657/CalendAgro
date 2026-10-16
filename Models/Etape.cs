namespace CalendAgro.Models;

// Période d'une phase pour une culture, une zone et une saison.
// MoisDebut et MoisFin inclus, de 1 (janvier) à 12 (décembre).
public sealed class Etape
{
    public required string CultureId { get; init; }

    public TypeEtape Type { get; init; }

    public required string Zone { get; init; }

    public required string Saison { get; init; }

    public int MoisDebut { get; init; }

    public int MoisFin { get; init; }
}
