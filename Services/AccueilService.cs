using CalendAgro.Models;

namespace CalendAgro.Services;

// Accueil calculé à partir des parcelles de l'utilisateur : compteurs, résumé des
// cultures et prochains travaux. Aucune donnée de démonstration.
public sealed class AccueilService : IAccueilService
{
    private readonly IParcelleService _parcelles;
    private readonly ICalendrierService _calendrier;
    private readonly ICultureService _cultures;

    public AccueilService(IParcelleService parcelles, ICalendrierService calendrier, ICultureService cultures)
    {
        _parcelles = parcelles;
        _calendrier = calendrier;
        _cultures = cultures;
    }

    public async Task<AccueilDonnees> GetAccueilAsync()
    {
        var parcelles = await _parcelles.GetParcellesAsync().ConfigureAwait(false);
        var travaux = parcelles
            .Where(p => p.ProchainTravail is not null)
            .Select(p => p.ProchainTravail!)
            .OrderBy(t => t.Date)
            .ToArray();
        var cultures = parcelles
            .Select(p => p.CultureNom)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        return new AccueilDonnees(
            travaux.Length,
            parcelles.Count,
            string.Join(" · ", cultures),
            parcelles.Sum(p => p.SurfaceHa),
            travaux);
    }

    public async Task<DetailTravail?> GetDetailAsync(string travailId)
    {
        if (!TravailId.TryLire(travailId, out var parcelleId, out var phase))
        {
            return null;
        }

        var parcelle = await _parcelles.GetParcelleAsync(parcelleId).ConfigureAwait(false);
        if (parcelle is null)
        {
            return null;
        }

        var conseils = await _cultures.GetConseilsAsync(parcelle.CultureId).ConfigureAwait(false);
        var phasesTerminees = await _parcelles.GetPhasesTermineesAsync(parcelleId).ConfigureAwait(false);

        return new DetailTravail
        {
            Id = travailId,
            Titre = CalendrierService.TitreDe(phase),
            Accroche = CalendrierService.AccrocheDe(phase),
            Phase = phase,
            Terminee = phasesTerminees.Contains(phase),
            DatePrevue = await _calendrier.GetDatePhaseAsync(parcelle, phase).ConfigureAwait(false)
                ?? parcelle.DateSemis,
            ParcelleNom = parcelle.Nom,
            CultureNom = parcelle.CultureNom,
            SurfaceHa = parcelle.SurfaceHa,
            Conseils = conseils
                .Where(c => c.Phase == phase)
                .OrderBy(c => c.Ordre)
                .Select(c => new ConseilBloc(c.Titre, c.Texte))
                .ToArray(),
        };
    }

    public async Task MarquerTermineeAsync(string travailId)
    {
        if (!TravailId.TryLire(travailId, out var parcelleId, out var phase))
        {
            return;
        }

        var parcelle = await _parcelles.GetParcelleAsync(parcelleId).ConfigureAwait(false);
        if (parcelle is null)
        {
            return;
        }

        var datePrevue = await _calendrier.GetDatePhaseAsync(parcelle, phase).ConfigureAwait(false)
            ?? parcelle.DateSemis;
        await _parcelles.MarquerTermineeAsync(parcelleId, phase, datePrevue).ConfigureAwait(false);
    }
}
