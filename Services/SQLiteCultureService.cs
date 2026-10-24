using CalendAgro.Data;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using SQLite;

namespace CalendAgro.Services;

// Cultures, étapes et conseils en SQLite : tout est local, aucun réseau.
// Le jeu de départ est importé au premier accès (JeuDeDepart), puis l'utilisateur
// modifie ses données. Les suppressions sont logiques (AGENTS §6 bis).
// Un catalogue vide reste un état normal si l'utilisateur a tout supprimé.
public sealed class SQLiteCultureService : ICultureService
{
    private const int JoursParMois = 30;
    private const int MoisParAn = 12;
    private const int AnsMinimumAffiche = 2;

    private readonly AppDatabase _donnees;

    public SQLiteCultureService(AppDatabase donnees) => _donnees = donnees;

    // Ouvre la base et importe le jeu de départ au tout premier accès.
    private async Task<SQLiteAsyncConnection> ConnexionAsync()
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        await JeuDeDepart.AssurerAsync(connexion).ConfigureAwait(false);
        return connexion;
    }

    public async Task<IReadOnlyList<Culture>> GetCulturesAsync() => await SearchAsync(null).ConfigureAwait(false);

    public async Task<IReadOnlyList<Culture>> SearchAsync(string? texte)
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var cultures = (await connexion.Table<CultureRow>().ToListAsync().ConfigureAwait(false))
            .Where(c => !c.IsDeleted);

        var recherche = string.IsNullOrWhiteSpace(texte) ? null : CultureId.Normaliser(texte);
        if (recherche is not null)
        {
            cultures = cultures.Where(c => CultureId.Normaliser(c.Nom).Contains(recherche, StringComparison.Ordinal));
        }

        var etapes = (await connexion.Table<EtapeRow>().ToListAsync().ConfigureAwait(false))
            .Where(e => !e.IsDeleted)
            .ToLookup(e => e.CultureId, StringComparer.Ordinal);
        var conseils = (await connexion.Table<ConseilRow>().ToListAsync().ConfigureAwait(false))
            .Where(c => !c.IsDeleted)
            .ToLookup(c => c.CultureId, StringComparer.Ordinal);

        return cultures
            .OrderBy(c => c.Nom, StringComparer.CurrentCultureIgnoreCase)
            .Select(c => Convertir(c, etapes[c.Id], conseils[c.Id]))
            .ToArray();
    }

    public async Task<Culture?> GetCultureAsync(string cultureId)
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<CultureRow>(cultureId).ConfigureAwait(false);
        if (ligne is null || ligne.IsDeleted)
        {
            return null;
        }

        var etapes = await EtapesDeAsync(cultureId).ConfigureAwait(false);
        var conseils = await ConseilsDeAsync(cultureId).ConfigureAwait(false);
        return Convertir(ligne, etapes, conseils);
    }

    public async Task<IReadOnlyList<string>> GetZonesAsync()
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var zones = (await connexion.Table<SaisonRow>().ToListAsync().ConfigureAwait(false))
            .OrderBy(s => s.ZoneOrdre)
            .Select(s => s.Zone)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (zones.Count == 0)
        {
            // Base antérieure au référentiel régional : on déduit les zones des étapes.
            var etapes = await connexion.Table<EtapeRow>().ToListAsync().ConfigureAwait(false);
            zones.AddRange(etapes
                .Where(e => !e.IsDeleted)
                .Select(e => e.Zone)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(z => z, StringComparer.CurrentCultureIgnoreCase));
        }

        return zones;
    }

    public async Task<IReadOnlyList<string>> GetSaisonsAsync(string zone)
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var saisons = (await connexion.Table<SaisonRow>().ToListAsync().ConfigureAwait(false))
            .Where(s => s.Zone == zone)
            .OrderBy(s => s.Ordre)
            .Select(s => s.Nom)
            .ToList();

        saisons.AddRange(await SaisonsLibresAsync(connexion, zone, saisons).ConfigureAwait(false));
        return saisons;
    }

    // Saisons saisies par l'utilisateur, absentes du référentiel : sans elles,
    // ses étapes resteraient invisibles dans les sélecteurs.
    private static async Task<IReadOnlyList<string>> SaisonsLibresAsync(
        SQLiteAsyncConnection connexion,
        string? zone,
        IReadOnlyCollection<string> connues)
    {
        var etapes = await connexion.Table<EtapeRow>().ToListAsync().ConfigureAwait(false);
        return etapes
            .Where(e => !e.IsDeleted && (zone is null || e.Zone == zone))
            .Select(e => e.Saison)
            .Where(s => !connues.Contains(s, StringComparer.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public async Task<IReadOnlyList<Etape>> GetEtapesAsync(string cultureId, string zone, string saison)
    {
        var etapes = await EtapesDeAsync(cultureId).ConfigureAwait(false);
        return etapes
            .Where(e => e.Zone == zone && e.Saison == saison)
            .OrderBy(e => e.Type)
            .Select(ConvertirEtape)
            .ToArray();
    }

    public async Task<IReadOnlyList<Conseil>> GetConseilsAsync(string cultureId)
    {
        var conseils = await ConseilsDeAsync(cultureId).ConfigureAwait(false);
        return conseils
            .OrderBy(c => c.Phase)
            .ThenBy(c => c.Ordre)
            .Select(c => new Conseil
            {
                CultureId = c.CultureId,
                Phase = (TypeEtape)c.Phase,
                Titre = c.Titre,
                Texte = c.Texte,
                Ordre = c.Ordre,
            })
            .ToArray();
    }

    public async Task<Culture> EnregistrerCultureAsync(
        string? cultureId,
        string nom,
        CategorieCulture categorie,
        int dureeCycleMois)
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var nomNettoye = nom.Trim();
        var identifiantNom = CultureId.Creer(nomNettoye);

        // Le nom doit rester unique : l'identifiant des parcelles en dérive.
        var meme = await connexion.FindAsync<CultureRow>(identifiantNom).ConfigureAwait(false);
        if (meme is { IsDeleted: false } && meme.Id != cultureId)
        {
            throw new CultureExistanteException();
        }

        CultureRow ligne;
        if (cultureId is null)
        {
            // Une culture supprimée du même nom est ressuscitée, sans ses anciennes données.
            ligne = new CultureRow { Id = identifiantNom, IconeKey = "sprout" };
        }
        else
        {
            ligne = await connexion.FindAsync<CultureRow>(cultureId).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Culture introuvable.");
        }

        // Cycle inchangé (arrondi au mois) : on garde le texte d'origine, par exemple « 3 à 4 mois ».
        var cycleInchange = cultureId is not null
            && (int)Math.Round(ligne.DureeCycleJours / (double)JoursParMois) == dureeCycleMois;
        if (!cycleInchange)
        {
            ligne.DureeCycleJours = dureeCycleMois * JoursParMois;
            ligne.CycleTexte = TexteCycle(dureeCycleMois);
        }

        ligne.Nom = nomNettoye;
        ligne.Categorie = (int)categorie;
        ligne.IsDeleted = false;
        ligne.UpdatedAt = DateTime.UtcNow.Ticks;
        await connexion.InsertOrReplaceAsync(ligne).ConfigureAwait(false);

        return Convertir(ligne, await EtapesDeAsync(ligne.Id).ConfigureAwait(false), await ConseilsDeAsync(ligne.Id).ConfigureAwait(false));
    }

    public async Task SupprimerCultureAsync(string cultureId)
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<CultureRow>(cultureId).ConfigureAwait(false);
        if (ligne is null)
        {
            return;
        }

        var maintenant = DateTime.UtcNow.Ticks;
        var etapes = await EtapesDeAsync(cultureId).ConfigureAwait(false);
        var conseils = await ConseilsDeAsync(cultureId).ConfigureAwait(false);
        await connexion.RunInTransactionAsync(transaction =>
        {
            ligne.IsDeleted = true;
            ligne.UpdatedAt = maintenant;
            transaction.Update(ligne);
            foreach (var etape in etapes)
            {
                etape.IsDeleted = true;
                etape.UpdatedAt = maintenant;
                transaction.Update(etape);
            }

            foreach (var conseil in conseils)
            {
                conseil.IsDeleted = true;
                conseil.UpdatedAt = maintenant;
                transaction.Update(conseil);
            }
        }).ConfigureAwait(false);
    }

    public async Task EnregistrerEtapeAsync(
        string cultureId,
        TypeEtape type,
        string zone,
        string saison,
        int moisDebut,
        int moisFin)
    {
        if (moisDebut is < 1 or > MoisParAn || moisFin is < 1 or > MoisParAn)
        {
            throw new ArgumentOutOfRangeException(nameof(moisDebut), "Les mois vont de 1 à 12.");
        }

        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var ligne = new EtapeRow
        {
            Id = JeuDeDepart.IdEtape(cultureId, type, zone, saison),
            CultureId = cultureId,
            Type = (int)type,
            Zone = zone,
            Saison = saison,
            MoisDebut = moisDebut,
            MoisFin = moisFin,
            UpdatedAt = DateTime.UtcNow.Ticks,
        };
        await connexion.InsertOrReplaceAsync(ligne).ConfigureAwait(false);
    }

    public async Task SupprimerEtapeAsync(string cultureId, TypeEtape type, string zone, string saison)
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion
            .FindAsync<EtapeRow>(JeuDeDepart.IdEtape(cultureId, type, zone, saison))
            .ConfigureAwait(false);
        if (ligne is null)
        {
            return;
        }

        ligne.IsDeleted = true;
        ligne.UpdatedAt = DateTime.UtcNow.Ticks;
        await connexion.UpdateAsync(ligne).ConfigureAwait(false);
    }

    public async Task EnregistrerConseilsAsync(string cultureId, TypeEtape phase, IReadOnlyList<string> textes)
    {
        var propres = textes
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .ToArray();
        if (propres.Any(t => !ConseilRegles.EstValide(t)))
        {
            throw new ArgumentException("Un conseil dépasse 12 mots.", nameof(textes));
        }

        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var anciens = (await ConseilsDeAsync(cultureId).ConfigureAwait(false))
            .Where(c => c.Phase == (int)phase)
            .ToArray();
        var maintenant = DateTime.UtcNow.Ticks;
        await connexion.RunInTransactionAsync(transaction =>
        {
            foreach (var ancien in anciens)
            {
                ancien.IsDeleted = true;
                ancien.UpdatedAt = maintenant;
                transaction.Update(ancien);
            }

            for (var i = 0; i < propres.Length; i++)
            {
                transaction.Insert(new ConseilRow
                {
                    Id = Guid.NewGuid().ToString("N"),
                    CultureId = cultureId,
                    Phase = (int)phase,
                    Titre = JeuDeDepart.TitreDe(phase),
                    Texte = propres[i],
                    Ordre = i,
                    UpdatedAt = maintenant,
                });
            }
        }).ConfigureAwait(false);
    }

    private static string TexteCycle(int mois) =>
        mois >= AnsMinimumAffiche * MoisParAn && mois % MoisParAn == 0
            ? string.Format(AppResources.Culture_CycleAns, mois / MoisParAn)
            : string.Format(AppResources.Culture_CycleMois, mois);

    private async Task<IReadOnlyList<EtapeRow>> EtapesDeAsync(string cultureId)
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var lignes = await connexion
            .Table<EtapeRow>()
            .Where(e => e.CultureId == cultureId)
            .ToListAsync()
            .ConfigureAwait(false);
        return lignes.Where(e => !e.IsDeleted).ToArray();
    }

    private async Task<IReadOnlyList<ConseilRow>> ConseilsDeAsync(string cultureId)
    {
        var connexion = await ConnexionAsync().ConfigureAwait(false);
        var lignes = await connexion
            .Table<ConseilRow>()
            .Where(c => c.CultureId == cultureId)
            .ToListAsync()
            .ConfigureAwait(false);
        return lignes.Where(c => !c.IsDeleted).ToArray();
    }

    private static Etape ConvertirEtape(EtapeRow ligne) => new()
    {
        CultureId = ligne.CultureId,
        Type = (TypeEtape)ligne.Type,
        Zone = ligne.Zone,
        Saison = ligne.Saison,
        MoisDebut = ligne.MoisDebut,
        MoisFin = ligne.MoisFin,
    };

    // La carte du catalogue montre la phase en cours ce mois-ci (sinon la première
    // phase connue, sinon le semis) et le premier conseil de cette phase.
    private static Culture Convertir(
        CultureRow ligne,
        IEnumerable<EtapeRow> etapes,
        IEnumerable<ConseilRow> conseils)
    {
        var mois = DateTime.Today.Month;
        var liste = etapes.ToArray();
        var phase = liste.Where(e => EstEnCours(e, mois)).Select(e => (TypeEtape?)e.Type).FirstOrDefault()
            ?? liste.OrderBy(e => e.Type).Select(e => (TypeEtape?)e.Type).FirstOrDefault()
            ?? TypeEtape.Semis;

        var tries = conseils.OrderBy(c => c.Ordre).ToArray();
        var apercu = tries.FirstOrDefault(c => c.Phase == (int)phase) ?? tries.FirstOrDefault();

        return new Culture
        {
            Id = ligne.Id,
            Nom = ligne.Nom,
            Categorie = (CategorieCulture)ligne.Categorie,
            DureeCycleJours = ligne.DureeCycleJours,
            CycleTexte = ligne.CycleTexte,
            IconeKey = ligne.IconeKey,
            ConseilApercu = apercu?.Texte ?? string.Empty,
            PhaseApercu = phase,
        };
    }

    // Une période peut enjamber la fin d'année (novembre à février).
    private static bool EstEnCours(EtapeRow etape, int mois) =>
        etape.MoisDebut <= etape.MoisFin
            ? mois >= etape.MoisDebut && mois <= etape.MoisFin
            : mois >= etape.MoisDebut || mois <= etape.MoisFin;
}
