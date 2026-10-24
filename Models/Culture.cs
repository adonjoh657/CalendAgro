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

    // Aperçu de conseil montré dans la carte du catalogue (12 mots max).
    public string ConseilApercu { get; init; } = string.Empty;

    // Phase montrée en badge sur la carte du catalogue.
    public TypeEtape PhaseApercu { get; init; }

    // Cycle court (< 6 mois) ou long, qualifié pour la carte du catalogue.
    public string CycleQualifier => DureeCycleJours < 180
        ? AppResources.Cultures_CycleCourt
        : AppResources.Cultures_CycleLong;

    public string CycleApercu => $"{CycleQualifier} · {CycleTexte}";

    public string PhaseLabel => TypeEtapeLabels.Libelle(PhaseApercu);
}
