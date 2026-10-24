using System.Globalization;
using CalendAgro.Data;
using CalendAgro.Models;
using SQLite;

namespace CalendAgro.Services;

// Rappels personnels en SQLite : la création et la suppression sont immédiates
// et fonctionnent hors ligne. Les colonnes de synchronisation sont prêtes
// pour la file d'attente Supabase (AGENTS §6 bis).
public sealed class SQLiteRappelPersonnelService : IRappelPersonnelService
{
    private const string FormatDate = "yyyy-MM-dd";
    private const string FormatHeure = "HH:mm";

    private readonly AppDatabase _donnees;

    public SQLiteRappelPersonnelService(AppDatabase donnees) => _donnees = donnees;

    public async Task<IReadOnlyList<RappelPersonnel>> GetRappelsAsync()
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var lignes = await connexion.Table<RappelPersonnelRow>().ToListAsync().ConfigureAwait(false);

        return lignes
            .Where(l => !l.IsDeleted)
            .Select(Convertir)
            .OrderBy(r => r.DateDebut)
            .ThenBy(r => r.Heure)
            .ThenBy(r => r.Titre, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public async Task<RappelPersonnel> AjouterAsync(
        string titre,
        DateOnly dateDebut,
        TimeOnly heure,
        FrequenceRappel frequence)
    {
        var ligne = new RappelPersonnelRow
        {
            Id = Guid.NewGuid().ToString("N"),
            Titre = titre.Trim(),
            DateDebut = dateDebut.ToString(FormatDate, CultureInfo.InvariantCulture),
            Heure = heure.ToString(FormatHeure, CultureInfo.InvariantCulture),
            Frequence = (int)frequence,
            Actif = true,
            UpdatedAt = DateTime.UtcNow.Ticks,
        };

        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        await connexion.InsertAsync(ligne).ConfigureAwait(false);
        return Convertir(ligne);
    }

    public async Task SetActifAsync(string rappelId, bool actif)
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<RappelPersonnelRow>(rappelId).ConfigureAwait(false);
        if (ligne is null)
        {
            return;
        }

        ligne.Actif = actif;
        ligne.UpdatedAt = DateTime.UtcNow.Ticks;
        await connexion.UpdateAsync(ligne).ConfigureAwait(false);
    }

    public async Task SupprimerAsync(string rappelId)
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<RappelPersonnelRow>(rappelId).ConfigureAwait(false);
        if (ligne is null)
        {
            return;
        }

        // Suppression logique : la ligne reste pour la synchronisation (AGENTS §6 bis).
        ligne.IsDeleted = true;
        ligne.Actif = false;
        ligne.UpdatedAt = DateTime.UtcNow.Ticks;
        await connexion.UpdateAsync(ligne).ConfigureAwait(false);
    }

    private static RappelPersonnel Convertir(RappelPersonnelRow ligne) => new()
    {
        Id = ligne.Id,
        Titre = ligne.Titre,
        DateDebut = DateOnly.ParseExact(ligne.DateDebut, FormatDate, CultureInfo.InvariantCulture),
        Heure = TimeOnly.ParseExact(ligne.Heure, FormatHeure, CultureInfo.InvariantCulture),
        Frequence = (FrequenceRappel)ligne.Frequence,
        Actif = ligne.Actif,
    };
}
