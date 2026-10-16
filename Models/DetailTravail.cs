namespace CalendAgro.Models;

// Bloc de conseil : titre + texte (exemple Figma, 12 mots maximum).
public sealed record ConseilBloc(string Titre, string Texte);

// Détail d'une tâche : exemple non officiel.
public sealed class DetailTravail
{
    public required string Id { get; init; }

    public required string Titre { get; init; }

    public required string Accroche { get; init; }

    public TypeEtape Phase { get; init; }

    public bool Terminee { get; set; }

    public DateOnly DatePrevue { get; init; }

    public required string ParcelleNom { get; init; }

    public required string CultureNom { get; init; }

    public double SurfaceHa { get; init; }

    public required IReadOnlyList<ConseilBloc> Conseils { get; init; }
}
