namespace CalendAgro.Models;

// Conseil court (12 mots maximum) rattaché à une phase d'une culture.
public sealed class Conseil
{
    public required string CultureId { get; init; }

    public TypeEtape Phase { get; init; }

    // Titre court du bloc affiché dans le détail d'une tâche.
    public required string Titre { get; init; }

    public required string Texte { get; init; }

    public int Ordre { get; init; }
}
