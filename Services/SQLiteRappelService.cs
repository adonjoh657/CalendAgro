using CalendAgro.Data;
using CalendAgro.Models;
using SQLite;

namespace CalendAgro.Services;

// Rappels calculés à partir des parcelles de l'utilisateur et du calendrier.
// Seul l'état actif est persisté : les dates restent recalculées hors ligne.
public sealed class SQLiteRappelService : IRappelService
{
    private readonly AppDatabase _donnees;
    private readonly IParcelleService _parcelles;
    private readonly ICalendrierService _calendrier;

    public SQLiteRappelService(
        AppDatabase donnees,
        IParcelleService parcelles,
        ICalendrierService calendrier)
    {
        _donnees = donnees;
        _parcelles = parcelles;
        _calendrier = calendrier;
    }

    public async Task<IReadOnlyList<Rappel>> GetRappelsAsync()
    {
        var parcelles = await _parcelles.GetParcellesAsync().ConfigureAwait(false);
        var etats = await ChargerEtatsAsync().ConfigureAwait(false);
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        var rappels = new List<Rappel>();

        foreach (var parcelle in parcelles)
        {
            var phasesTerminees = await _parcelles.GetPhasesTermineesAsync(parcelle.Id).ConfigureAwait(false);
            var travaux = await _calendrier.GetTravauxAsync(parcelle, phasesTerminees).ConfigureAwait(false);
            var phasesVues = new HashSet<TypeEtape>();
            foreach (var travail in travaux)
            {
                phasesVues.Add(travail.Phase);
                rappels.Add(Creer(travail, parcelle, etats));
            }

            // Une culture saisie sans étape n'a aucun calendrier : la date de semis
            // saisie est alors le seul repère, on l'utilise pour proposer un rappel
            // sans inventer de durée de cycle.
            if (!phasesVues.Contains(TypeEtape.Semis)
                && !phasesTerminees.Contains(TypeEtape.Semis)
                && parcelle.DateSemis >= aujourdHui)
            {
                var id = TravailId.Creer(parcelle.Id, TypeEtape.Semis);
                rappels.Add(new Rappel
                {
                    Id = id,
                    Titre = CalendrierService.TitreDe(TypeEtape.Semis),
                    ParcelleId = parcelle.Id,
                    ParcelleNom = parcelle.Nom,
                    CultureNom = parcelle.CultureNom,
                    Date = parcelle.DateSemis,
                    Phase = TypeEtape.Semis,
                    Actif = EstActif(etats, id),
                });
            }
        }

        return rappels
            .OrderBy(r => r.Date)
            .ThenBy(r => Ordre(r.Phase))
            .ThenBy(r => r.ParcelleNom, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public async Task SetActifAsync(string rappelId, bool actif)
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<RappelRow>(rappelId).ConfigureAwait(false)
            ?? new RappelRow { Id = rappelId };

        ligne.Actif = actif;
        ligne.IsDeleted = false;
        ligne.UpdatedAt = DateTime.UtcNow.Ticks;
        if (TravailId.TryLire(rappelId, out var parcelleId, out var phase))
        {
            ligne.ParcelleId = parcelleId;
            ligne.Phase = (int)phase;
        }

        await connexion.InsertOrReplaceAsync(ligne).ConfigureAwait(false);
    }

    private async Task<Dictionary<string, bool>> ChargerEtatsAsync()
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var lignes = await connexion.Table<RappelRow>().ToListAsync().ConfigureAwait(false);
        return lignes
            .Where(l => !l.IsDeleted)
            .ToDictionary(l => l.Id, l => l.Actif, StringComparer.Ordinal);
    }

    private static Rappel Creer(Travail travail, Parcelle parcelle, Dictionary<string, bool> etats) => new()
    {
        Id = travail.Id,
        Titre = travail.Titre,
        ParcelleId = parcelle.Id,
        ParcelleNom = parcelle.Nom,
        CultureNom = parcelle.CultureNom,
        Date = travail.Date,
        Phase = travail.Phase,
        Actif = EstActif(etats, travail.Id),
    };

    private static bool EstActif(Dictionary<string, bool> etats, string rappelId) =>
        !etats.TryGetValue(rappelId, out var actif) || actif;

    private static int Ordre(TypeEtape phase) => phase switch
    {
        TypeEtape.Semis => 0,
        TypeEtape.Entretien => 1,
        _ => 2,
    };
}
