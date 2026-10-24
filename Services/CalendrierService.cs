using CalendAgro.Models;
using CalendAgro.Resources.Strings;

namespace CalendAgro.Services;

// Dates des phases d'une parcelle : ancrées sur la date de semis saisie, avec les
// écarts entre phases tirés des étapes de référence (culture + zone + saison).
// Données d'exemple, non officielles : la logique suit le JSON, elle ne l'invente pas.
public sealed class CalendrierService : ICalendrierService
{
    private const int MoisParAn = 12;

    private readonly ICultureService _cultures;

    public CalendrierService(ICultureService cultures) => _cultures = cultures;

    public async Task<IReadOnlyList<Travail>> GetTravauxAsync(
        Parcelle parcelle,
        IReadOnlySet<TypeEtape> phasesTerminees)
    {
        var dates = await GetDatesAsync(parcelle).ConfigureAwait(false);
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);

        return dates
            .Where(p => p.Value >= aujourdHui && !phasesTerminees.Contains(p.Key))
            .OrderBy(p => p.Value)
            .ThenBy(p => Ordre(p.Key))
            .Select(p => CreerTravail(parcelle, p.Key, p.Value))
            .ToArray();
    }

    public async Task<DateOnly?> GetDatePhaseAsync(Parcelle parcelle, TypeEtape phase)
    {
        var dates = await GetDatesAsync(parcelle).ConfigureAwait(false);
        return dates.TryGetValue(phase, out var date) ? date : null;
    }

    internal static string TitreDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Semis => AppResources.Travail_Semis,
        TypeEtape.Recolte => AppResources.Travail_Recolte,
        _ => AppResources.Travail_Entretien,
    };

    internal static string AccrocheDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Semis => AppResources.Travail_SemisAccroche,
        TypeEtape.Recolte => AppResources.Travail_RecolteAccroche,
        _ => AppResources.Travail_EntretienAccroche,
    };

    // Une parcelle suit une seule campagne : celle dont la période de semis contient
    // la date de semis, sinon la campagne suivante la plus proche. Mélanger les
    // saisons donnait des dates qui ne correspondaient à aucune période de culture.
    private async Task<Dictionary<TypeEtape, DateOnly>> GetDatesAsync(Parcelle parcelle)
    {
        var dates = new Dictionary<TypeEtape, DateOnly>();
        var saison = await TrouverSaisonAsync(parcelle).ConfigureAwait(false);
        if (saison is null)
        {
            return dates;
        }

        var etapes = await _cultures
            .GetEtapesAsync(parcelle.CultureId, parcelle.Region, saison)
            .ConfigureAwait(false);
        var semis = etapes.FirstOrDefault(e => e.Type == TypeEtape.Semis);
        if (semis is null)
        {
            return dates;
        }

        foreach (var etape in etapes)
        {
            dates[etape.Type] = parcelle.DateSemis.AddMonths(EcartMois(semis.MoisDebut, etape.MoisDebut));
        }

        return dates;
    }

    private async Task<string?> TrouverSaisonAsync(Parcelle parcelle)
    {
        var mois = parcelle.DateSemis.Month;
        string? contenant = null;
        string? suivante = null;
        var distanceSuivante = int.MaxValue;

        foreach (var saison in await _cultures.GetSaisonsAsync(parcelle.Region).ConfigureAwait(false))
        {
            var etapes = await _cultures
                .GetEtapesAsync(parcelle.CultureId, parcelle.Region, saison)
                .ConfigureAwait(false);
            var semis = etapes.FirstOrDefault(e => e.Type == TypeEtape.Semis);
            if (semis is null)
            {
                continue;
            }

            if (DansPeriode(mois, semis.MoisDebut, semis.MoisFin))
            {
                contenant ??= saison;
            }

            var distance = (semis.MoisDebut - mois + MoisParAn) % MoisParAn;
            if (distance < distanceSuivante)
            {
                distanceSuivante = distance;
                suivante = saison;
            }
        }

        return contenant ?? suivante;
    }

    private static Travail CreerTravail(Parcelle parcelle, TypeEtape phase, DateOnly date) => new()
    {
        Id = TravailId.Creer(parcelle.Id, phase),
        Titre = TitreDe(phase),
        ParcelleNom = parcelle.Nom,
        CultureNom = parcelle.CultureNom,
        Date = date,
        Phase = phase,
    };

    private static int EcartMois(int moisReference, int mois) => ((mois - moisReference) + MoisParAn) % MoisParAn;

    // Une période peut enjamber la fin d'année (novembre à février).
    private static bool DansPeriode(int mois, int debut, int fin) =>
        debut <= fin
            ? mois >= debut && mois <= fin
            : mois >= debut || mois <= fin;

    private static int Ordre(TypeEtape phase) => phase switch
    {
        TypeEtape.Semis => 0,
        TypeEtape.Entretien => 1,
        _ => 2,
    };
}
