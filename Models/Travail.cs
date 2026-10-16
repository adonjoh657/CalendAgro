namespace CalendAgro.Models;

// Travail à venir : exemple non officiel, dates relatives à aujourd'hui.
public sealed class Travail
{
    public required string Id { get; init; }

    public required string Titre { get; init; }

    public required string ParcelleNom { get; init; }

    public required string CultureNom { get; init; }

    public DateOnly Date { get; init; }

    public TypeEtape Phase { get; init; }
}
