using CalendAgro.Models;

namespace CalendAgro.Services;

// Données de référence : cultures, recherche insensible aux accents,
// filtre par catégorie. JSON embarqué plus tard ; factice pour la phase B.
public interface ICultureService
{
    Task<IReadOnlyList<Culture>> GetCulturesAsync();

    Task<IReadOnlyList<Culture>> SearchAsync(string? texte, CategorieCulture? categorie);

    Task<Culture?> GetCultureAsync(string cultureId);

    Task<IReadOnlyList<Etape>> GetEtapesAsync(string cultureId);

    Task<IReadOnlyList<Conseil>> GetConseilsAsync(string cultureId);
}
