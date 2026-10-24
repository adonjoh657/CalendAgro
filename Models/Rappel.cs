namespace CalendAgro.Models;

// Rappel de saison : un travail à venir d'une parcelle, activable par l'utilisateur.
public sealed class Rappel
{
    public required string Id { get; init; }

    public required string Titre { get; init; }

    public required string ParcelleId { get; init; }

    public required string ParcelleNom { get; init; }

    public required string CultureNom { get; init; }

    public DateOnly Date { get; init; }

    public TypeEtape Phase { get; init; }

    public bool Actif { get; init; }
}
