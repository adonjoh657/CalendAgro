namespace CalendAgro.Models;

// Parcelle de l'utilisateur : exemple local, stockée en SQLite plus tard.
public sealed class Parcelle
{
    public required string Id { get; init; }

    public required string Nom { get; init; }

    public required string CultureId { get; init; }

    public required string CultureNom { get; init; }

    public double SurfaceHa { get; init; }

    public required string Region { get; init; }

    public DateOnly DateSemis { get; init; }

    public Travail? ProchainTravail { get; init; }
}
