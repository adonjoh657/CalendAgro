using CalendAgro.Models;

namespace CalendAgro.Services;

// Cultures, étapes et conseils : lecture, recherche insensible aux accents,
// et saisie par l'utilisateur. Tout est local (SQLite).
public interface ICultureService
{
    Task<IReadOnlyList<Culture>> GetCulturesAsync();

    Task<IReadOnlyList<Culture>> SearchAsync(string? texte);

    Task<Culture?> GetCultureAsync(string cultureId);

    Task<IReadOnlyList<string>> GetZonesAsync();

    // Saisons de la région, dans l'ordre du référentiel.
    Task<IReadOnlyList<string>> GetSaisonsAsync(string zone);

    Task<IReadOnlyList<Etape>> GetEtapesAsync(string cultureId, string zone, string saison);

    Task<IReadOnlyList<Conseil>> GetConseilsAsync(string cultureId);

    // Crée la culture (cultureId nul) ou la modifie. L'identifiant ne change jamais
    // à la modification, pour rester rattaché aux parcelles.
    // Lève CultureExistanteException si le nom est déjà pris.
    Task<Culture> EnregistrerCultureAsync(
        string? cultureId,
        string nom,
        CategorieCulture categorie,
        int dureeCycleMois);

    // Suppression logique de la culture, de ses étapes et de ses conseils.
    Task SupprimerCultureAsync(string cultureId);

    Task EnregistrerEtapeAsync(
        string cultureId,
        TypeEtape type,
        string zone,
        string saison,
        int moisDebut,
        int moisFin);

    Task SupprimerEtapeAsync(string cultureId, TypeEtape type, string zone, string saison);

    // Remplace les conseils d'une phase (12 mots au maximum chacun).
    Task EnregistrerConseilsAsync(string cultureId, TypeEtape phase, IReadOnlyList<string> textes);
}
