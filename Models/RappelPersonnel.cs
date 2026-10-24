namespace CalendAgro.Models;

// Rappel saisi par l'utilisateur, indépendant d'une parcelle. Il se répète
// selon sa fréquence jusqu'à sa désactivation ou sa suppression.
public sealed class RappelPersonnel
{
    public required string Id { get; init; }

    public required string Titre { get; init; }

    public DateOnly DateDebut { get; init; }

    public TimeOnly Heure { get; init; }

    public FrequenceRappel Frequence { get; init; }

    public bool Actif { get; set; }
}
