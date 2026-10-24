namespace CalendAgro.Models;

// Période d'une phase pour une parcelle : du mois de début au mois de fin de
// l'étape de référence, ancrée sur la date de semis saisie.
public sealed class PeriodeTravail
{
    public required string Id { get; init; }

    public required string Titre { get; init; }

    public required string ParcelleNom { get; init; }

    public required string CultureNom { get; init; }

    public DateOnly Debut { get; init; }

    public DateOnly Fin { get; init; }

    public TypeEtape Phase { get; init; }

    public bool Contient(DateOnly date) => date >= Debut && date <= Fin;
}
