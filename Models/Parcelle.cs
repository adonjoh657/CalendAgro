namespace CalendAgro.Models;

// Parcelle de l'utilisateur, stockée en SQLite (données personnelles locales).
public sealed class Parcelle
{
    public required string Id { get; init; }

    public required string Nom { get; init; }

    public required string CultureId { get; init; }

    public required string CultureNom { get; init; }

    public double SurfaceHa { get; init; }

    public required string Region { get; init; }

    public DateOnly DateSemis { get; init; }

    // Rempli à la lecture par le service des parcelles, jamais stocké tel quel.
    public Travail? ProchainTravail { get; set; }
}
