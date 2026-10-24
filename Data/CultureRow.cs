using SQLite;

namespace CalendAgro.Data;

// Ligne SQLite d'une culture. Les cultures sont des données utilisateur :
// le jeu de départ (PLAN C2) et les cultures ajoutées à la main vivent dans la même table.
[Table("Cultures")]
public sealed class CultureRow
{
    // Identifiant stable dérivé du nom (voir CultureId), identique à celui des parcelles.
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Nom { get; set; } = string.Empty;

    // Valeur de CategorieCulture.
    public int Categorie { get; set; }

    public int DureeCycleJours { get; set; }

    // Texte d'affichage du cycle (« 3 à 4 mois »).
    public string CycleTexte { get; set; } = string.Empty;

    public string IconeKey { get; set; } = string.Empty;

    // Vrai pour une culture du jeu de départ, faux pour une culture saisie par l'utilisateur.
    public bool EstDeDepart { get; set; }

    // Colonnes de synchronisation (AGENTS §6 bis).
    public long UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
