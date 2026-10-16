using System.Globalization;
using System.Text;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;

namespace CalendAgro.Services;

// 9 cultures du cahier des charges. Durées plausibles, NON officielles :
// à valider (IRAD, MINADER) avant toute diffusion.
public sealed class FakeCultureService : ICultureService
{
    private static readonly Culture[] Donnees =
    [
        new() { Id = "mais", Nom = "Maïs", Categorie = CategorieCulture.Cereale, DureeCycleJours = 120, CycleTexte = "3 à 4 mois", IconeKey = "mais" },
        new() { Id = "manioc", Nom = "Manioc", Categorie = CategorieCulture.Tubercule, DureeCycleJours = 365, CycleTexte = "9 à 18 mois", IconeKey = "manioc" },
        new() { Id = "plantain", Nom = "Plantain", Categorie = CategorieCulture.Tubercule, DureeCycleJours = 400, CycleTexte = "9 à 12 mois", IconeKey = "plantain" },
        new() { Id = "arachide", Nom = "Arachide", Categorie = CategorieCulture.Rente, DureeCycleJours = 120, CycleTexte = "3 à 4 mois", IconeKey = "arachide" },
        new() { Id = "tomate", Nom = "Tomate", Categorie = CategorieCulture.Legume, DureeCycleJours = 100, CycleTexte = "3 à 4 mois", IconeKey = "tomate" },
        new() { Id = "haricot", Nom = "Haricot", Categorie = CategorieCulture.Legume, DureeCycleJours = 90, CycleTexte = "2 à 3 mois", IconeKey = "haricot" },
        new() { Id = "cacao", Nom = "Cacao", Categorie = CategorieCulture.Rente, DureeCycleJours = 1825, CycleTexte = "3 ans et plus", IconeKey = "cacao" },
        new() { Id = "igname", Nom = "Igname", Categorie = CategorieCulture.Tubercule, DureeCycleJours = 240, CycleTexte = "8 à 10 mois", IconeKey = "igname" },
        new() { Id = "piment", Nom = "Piment", Categorie = CategorieCulture.Legume, DureeCycleJours = 110, CycleTexte = "3 à 4 mois", IconeKey = "piment" },
    ];

    // Phases d'exemple (zone Centre) : mêmes mois pour toutes les cultures,
    // NON officielles. Conseils courts génériques, 12 mots maximum.
    // Deux cycles par an (maquette fiche culture) : Mar+Août, Avr-Mai+Sep-Oct, Jun-Jul+Nov-Déc.
    private static readonly Etape[] Etapes =
    [
        new() { CultureId = "mais", Type = TypeEtape.Semis, Zone = "Centre", Saison = "Petite saison des pluies", MoisDebut = 3, MoisFin = 3 },
        new() { CultureId = "mais", Type = TypeEtape.Semis, Zone = "Centre", Saison = "Grande saison des pluies", MoisDebut = 8, MoisFin = 8 },
        new() { CultureId = "mais", Type = TypeEtape.Entretien, Zone = "Centre", Saison = "Petite saison des pluies", MoisDebut = 4, MoisFin = 5 },
        new() { CultureId = "mais", Type = TypeEtape.Entretien, Zone = "Centre", Saison = "Grande saison des pluies", MoisDebut = 9, MoisFin = 10 },
        new() { CultureId = "mais", Type = TypeEtape.Recolte, Zone = "Centre", Saison = "Petite saison des pluies", MoisDebut = 6, MoisFin = 7 },
        new() { CultureId = "mais", Type = TypeEtape.Recolte, Zone = "Centre", Saison = "Grande saison des pluies", MoisDebut = 11, MoisFin = 12 },
    ];

    private static readonly Conseil[] Conseils =
    [
        new() { CultureId = "mais", Phase = TypeEtape.Semis, Texte = "Semez lorsque les pluies sont installées et le sol humide.", Ordre = 1 },
        new() { CultureId = "mais", Phase = TypeEtape.Semis, Texte = "Respectez les espacements entre les poquets.", Ordre = 2 },
        new() { CultureId = "mais", Phase = TypeEtape.Entretien, Texte = "Désherbez tôt. Observez les plants avant chaque intervention.", Ordre = 1 },
        new() { CultureId = "mais", Phase = TypeEtape.Entretien, Texte = "Surveillez les feuilles chaque semaine.", Ordre = 2 },
        new() { CultureId = "mais", Phase = TypeEtape.Recolte, Texte = "Vérifiez la maturité avant de récolter.", Ordre = 1 },
        new() { CultureId = "mais", Phase = TypeEtape.Recolte, Texte = "Séchez puis stockez au sec.", Ordre = 2 },
    ];

    public async Task<IReadOnlyList<Culture>> GetCulturesAsync()
    {
        await Task.Delay(400);
        return Donnees;
    }

    public async Task<Culture?> GetCultureAsync(string cultureId)
    {
        await Task.Delay(200);
        return Donnees.FirstOrDefault(c => c.Id == cultureId);
    }

    public Task<IReadOnlyList<string>> GetZonesAsync() =>
        Task.FromResult<IReadOnlyList<string>>(
        [
            AppResources.Fiche_ZoneLittoral,
            AppResources.Fiche_ZoneCentre,
            AppResources.Fiche_ZoneOuest,
            AppResources.Fiche_ZoneExtremeNord,
        ]);

    public Task<IReadOnlyList<string>> GetSaisonsAsync() =>
        Task.FromResult<IReadOnlyList<string>>(
        [
            AppResources.Fiche_SaisonGrandeSeche,
            AppResources.Fiche_SaisonPetitePluies,
            AppResources.Fiche_SaisonPetiteSeche,
            AppResources.Fiche_SaisonGrandePluies,
        ]);

    public async Task<IReadOnlyList<Etape>> GetEtapesAsync(string cultureId, string zone, string saison)
    {
        await Task.Delay(200);
        // Exemple maïs appliqué à toutes les cultures en phase B.
        // Les périodes glissent selon la zone et la saison (logique provisoire).
        var zones = await GetZonesAsync();
        var saisons = await GetSaisonsAsync();
        var decalage = (Math.Max(0, zones.ToList().IndexOf(zone)) + Math.Max(0, saisons.ToList().IndexOf(saison))) % 12;
        return Etapes.Select(e =>
        {
            var (debut, fin) = Decaler(e.MoisDebut, e.MoisFin, decalage);
            return new Etape
            {
                CultureId = cultureId,
                Type = e.Type,
                Zone = zone,
                Saison = saison,
                MoisDebut = debut,
                MoisFin = fin,
            };
        }).ToArray();
    }

    private static (int Debut, int Fin) Decaler(int debut, int fin, int decalage)
    {
        var nouveauDebut = ((debut - 1 + decalage) % 12) + 1;
        var longueur = fin >= debut ? fin - debut : fin - debut + 12;
        var nouveauFin = ((nouveauDebut - 1 + longueur) % 12) + 1;
        return (nouveauDebut, nouveauFin);
    }

    public async Task<IReadOnlyList<Conseil>> GetConseilsAsync(string cultureId)
    {
        await Task.Delay(200);
        return Conseils.Select(c => new Conseil
        {
            CultureId = cultureId,
            Phase = c.Phase,
            Texte = c.Texte,
            Ordre = c.Ordre,
        }).ToArray();
    }

    public Task<IReadOnlyList<Culture>> SearchAsync(string? texte, CategorieCulture? categorie)
    {
        var filtre = Normaliser(texte ?? string.Empty);
        IReadOnlyList<Culture> resultats = Donnees
            .Where(c => (categorie is null || c.Categorie == categorie)
                && (filtre.Length == 0 || Normaliser(c.Nom).Contains(filtre, StringComparison.Ordinal)))
            .ToArray();
        return Task.FromResult(resultats);
    }

    private static string Normaliser(string valeur) => string.Concat(valeur
        .Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        .Select(char.ToLowerInvariant));
}
