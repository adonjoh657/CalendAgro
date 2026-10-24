using System.Globalization;
using CalendAgro.Data;
using CalendAgro.Models;
using SQLite;

namespace CalendAgro.Services;

// Parcelles en SQLite : l'écriture locale est immédiate et ne dépend d'aucun réseau.
// Les colonnes UserId, UpdatedAt et IsDeleted sont prêtes pour la file de
// synchronisation Supabase (phase E, dossier Sync/).
public sealed class SQLiteParcelleService : IParcelleService
{
    private const string FormatDate = "yyyy-MM-dd";

    private readonly AppDatabase _donnees;
    private readonly ICalendrierService _calendrier;

    public SQLiteParcelleService(AppDatabase donnees, ICalendrierService calendrier)
    {
        _donnees = donnees;
        _calendrier = calendrier;
    }

    public async Task<IReadOnlyList<Parcelle>> GetParcellesAsync()
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var lignes = await connexion.Table<ParcelleRow>().ToListAsync().ConfigureAwait(false);
        var phasesTerminees = await ChargerPhasesTermineesAsync(connexion).ConfigureAwait(false);

        var parcelles = new List<Parcelle>();
        foreach (var ligne in lignes
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.Nom, StringComparer.CurrentCultureIgnoreCase))
        {
            parcelles.Add(await ConvertirAsync(ligne, PhasesDe(phasesTerminees, ligne.Id)).ConfigureAwait(false));
        }

        return parcelles;
    }

    public async Task<Parcelle?> GetParcelleAsync(string parcelleId)
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<ParcelleRow>(parcelleId).ConfigureAwait(false);
        if (ligne is null || ligne.IsDeleted)
        {
            return null;
        }

        return await ConvertirAsync(ligne, await GetPhasesTermineesAsync(parcelleId).ConfigureAwait(false))
            .ConfigureAwait(false);
    }

    public async Task<Parcelle> AjouterAsync(
        string nom,
        string cultureNom,
        double? surfaceHa,
        string region,
        DateOnly dateSemis)
    {
        var ligne = new ParcelleRow
        {
            Id = Guid.NewGuid().ToString("N"),
            Nom = nom.Trim(),
            CultureId = CultureId.Creer(cultureNom),
            CultureNom = cultureNom.Trim(),
            SurfaceHa = surfaceHa ?? 0,
            Region = region.Trim(),
            DateSemis = EcrireDate(dateSemis),
            UpdatedAt = DateTime.UtcNow.Ticks,
        };

        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        await connexion.InsertAsync(ligne).ConfigureAwait(false);
        return await ConvertirAsync(ligne, new HashSet<TypeEtape>()).ConfigureAwait(false);
    }

    public async Task SupprimerAsync(string parcelleId)
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<ParcelleRow>(parcelleId).ConfigureAwait(false);
        if (ligne is null)
        {
            return;
        }

        // Suppression logique : la ligne reste pour la synchronisation (AGENTS §6 bis).
        ligne.IsDeleted = true;
        ligne.UpdatedAt = DateTime.UtcNow.Ticks;
        await connexion.UpdateAsync(ligne).ConfigureAwait(false);
    }

    public async Task<IReadOnlySet<TypeEtape>> GetPhasesTermineesAsync(string parcelleId)
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var lignes = await connexion
            .Table<TravailTermineRow>()
            .Where(t => t.ParcelleId == parcelleId)
            .ToListAsync()
            .ConfigureAwait(false);

        var phases = new HashSet<TypeEtape>();
        foreach (var ligne in lignes.Where(l => !l.IsDeleted))
        {
            phases.Add((TypeEtape)ligne.Phase);
        }

        return phases;
    }

    public async Task MarquerTermineeAsync(string parcelleId, TypeEtape phase, DateOnly datePrevue)
    {
        var ligne = new TravailTermineRow
        {
            Id = TravailId.Creer(parcelleId, phase),
            ParcelleId = parcelleId,
            Phase = (int)phase,
            DateTerminee = EcrireDate(datePrevue),
            UpdatedAt = DateTime.UtcNow.Ticks,
        };

        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        await connexion.InsertOrReplaceAsync(ligne).ConfigureAwait(false);
    }

    private async Task<Parcelle> ConvertirAsync(ParcelleRow ligne, IReadOnlySet<TypeEtape> phasesTerminees)
    {
        var parcelle = new Parcelle
        {
            Id = ligne.Id,
            Nom = ligne.Nom,
            CultureId = ligne.CultureId,
            CultureNom = ligne.CultureNom,
            SurfaceHa = ligne.SurfaceHa,
            Region = ligne.Region,
            DateSemis = LireDate(ligne.DateSemis),
        };

        var travaux = await _calendrier.GetTravauxAsync(parcelle, phasesTerminees).ConfigureAwait(false);
        if (travaux.Count > 0)
        {
            parcelle.ProchainTravail = travaux[0];
        }

        return parcelle;
    }

    private static async Task<Dictionary<string, HashSet<TypeEtape>>> ChargerPhasesTermineesAsync(
        SQLiteAsyncConnection connexion)
    {
        var lignes = await connexion.Table<TravailTermineRow>().ToListAsync().ConfigureAwait(false);
        var resultat = new Dictionary<string, HashSet<TypeEtape>>(StringComparer.Ordinal);
        foreach (var ligne in lignes.Where(l => !l.IsDeleted))
        {
            if (!resultat.TryGetValue(ligne.ParcelleId, out var phases))
            {
                phases = [];
                resultat[ligne.ParcelleId] = phases;
            }

            phases.Add((TypeEtape)ligne.Phase);
        }

        return resultat;
    }

    private static IReadOnlySet<TypeEtape> PhasesDe(
        Dictionary<string, HashSet<TypeEtape>> phasesTerminees,
        string parcelleId) =>
        phasesTerminees.TryGetValue(parcelleId, out var phases) ? phases : new HashSet<TypeEtape>();

    private static string EcrireDate(DateOnly date) =>
        date.ToString(FormatDate, CultureInfo.InvariantCulture);

    private static DateOnly LireDate(string valeur) =>
        DateOnly.ParseExact(valeur, FormatDate, CultureInfo.InvariantCulture);
}
