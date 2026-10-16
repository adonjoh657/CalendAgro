using CalendAgro.Resources.Strings;

namespace CalendAgro.Models;

// Donnée de référence (lecture seule). Libellés via resx, jamais en dur.
public sealed class Culture
{
    public required string Id { get; init; }

    public required string Nom { get; init; }

    public CategorieCulture Categorie { get; init; }

    public int DureeCycleJours { get; init; }

    // Texte d'affichage du cycle (« 3 à 4 mois »), exemple non officiel.
    public required string CycleTexte { get; init; }

    public required string IconeKey { get; init; }

    public string CategorieLabel => Categorie switch
    {
        CategorieCulture.Cereale => AppResources.Cultures_Cereale,
        CategorieCulture.Tubercule => AppResources.Cultures_Tubercule,
        CategorieCulture.Legume => AppResources.Cultures_Legume,
        _ => AppResources.Cultures_Rente,
    };

    public string CycleLabel => string.Format(AppResources.Cultures_CycleJours, DureeCycleJours);

    public string MetaLabel => $"{CategorieLabel} · {CycleLabel}";
}
