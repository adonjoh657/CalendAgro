using System.Text.Json;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using Microsoft.Maui.Storage;
using SQLite;

namespace CalendAgro.Data;

// Jeu de départ : 9 cultures d'exemple (non officielles) lues dans Resources/Raw/cultures.json
// et copiées dans SQLite au premier lancement. Ensuite l'utilisateur les modifie librement ;
// elles ne sont jamais réimportées, même s'il les supprime.
public static class JeuDeDepart
{
    private const string Fichier = "cultures.json";
    private const string CleImport = "cultures_depart_importe";

    private static readonly SemaphoreSlim Verrou = new(1, 1);

    public static async Task AssurerAsync(SQLiteAsyncConnection connexion)
    {
        if (Preferences.Get(CleImport, false))
        {
            return;
        }

        await Verrou.WaitAsync().ConfigureAwait(false);
        try
        {
            if (Preferences.Get(CleImport, false))
            {
                return;
            }

            using var flux = await FileSystem.OpenAppPackageFileAsync(Fichier).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(flux).ConfigureAwait(false);
            var lignes = Lire(document.RootElement);

            await connexion.RunInTransactionAsync(transaction =>
            {
                foreach (var saison in lignes.Saisons)
                {
                    transaction.InsertOrReplace(saison);
                }

                foreach (var culture in lignes.Cultures)
                {
                    transaction.InsertOrReplace(culture);
                }

                foreach (var etape in lignes.Etapes)
                {
                    transaction.InsertOrReplace(etape);
                }

                foreach (var conseil in lignes.Conseils)
                {
                    transaction.InsertOrReplace(conseil);
                }
            }).ConfigureAwait(false);

            Preferences.Set(CleImport, true);
        }
        finally
        {
            Verrou.Release();
        }
    }

    private static (List<SaisonRow> Saisons, List<CultureRow> Cultures, List<EtapeRow> Etapes, List<ConseilRow> Conseils) Lire(
        JsonElement racine)
    {
        var date = DateOnly.ParseExact(racine.GetProperty("miseAJour").GetString()!, "yyyy-MM-dd");
        var horodatage = date.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc).Ticks;

        var saisons = new List<SaisonRow>();
        var cultures = new List<CultureRow>();
        var etapes = new List<EtapeRow>();
        var conseils = new List<ConseilRow>();

        var ordreZone = 0;
        foreach (var region in racine.GetProperty("regions").EnumerateArray())
        {
            var zone = region.GetProperty("nom").GetString()!;
            var ordre = 0;
            foreach (var saison in region.GetProperty("saisons").EnumerateArray())
            {
                var nom = saison.GetString()!;
                saisons.Add(new SaisonRow
                {
                    Id = IdSaison(zone, nom),
                    Zone = zone,
                    Nom = nom,
                    ZoneOrdre = ordreZone,
                    Ordre = ordre++,
                });
            }

            ordreZone++;
        }

        foreach (var element in racine.GetProperty("cultures").EnumerateArray())
        {
            var id = element.GetProperty("id").GetString()!;
            cultures.Add(new CultureRow
            {
                Id = id,
                Nom = element.GetProperty("nom").GetString()!,
                Categorie = (int)LireCategorie(element.GetProperty("categorie").GetString()!),
                DureeCycleJours = element.GetProperty("dureeCycleJours").GetInt32(),
                CycleTexte = element.GetProperty("cycleTexte").GetString()!,
                IconeKey = "sprout",
                EstDeDepart = true,
                UpdatedAt = horodatage,
            });

            foreach (var e in element.GetProperty("etapes").EnumerateArray())
            {
                var phase = LirePhase(e.GetProperty("phase").GetString()!);
                var zone = e.GetProperty("zone").GetString()!;
                var saison = e.GetProperty("saison").GetString()!;
                etapes.Add(new EtapeRow
                {
                    Id = IdEtape(id, phase, zone, saison),
                    CultureId = id,
                    Type = (int)phase,
                    Zone = zone,
                    Saison = saison,
                    MoisDebut = e.GetProperty("debut").GetInt32(),
                    MoisFin = e.GetProperty("fin").GetInt32(),
                    UpdatedAt = horodatage,
                });
            }

            if (element.TryGetProperty("conseils", out var groupes))
            {
                foreach (var groupe in groupes.EnumerateObject())
                {
                    var phase = LirePhase(groupe.Name);
                    var ordre = 0;
                    foreach (var texte in groupe.Value.EnumerateArray())
                    {
                        conseils.Add(new ConseilRow
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            CultureId = id,
                            Phase = (int)phase,
                            Titre = TitreDe(phase),
                            Texte = texte.GetString()!,
                            Ordre = ordre++,
                            UpdatedAt = horodatage,
                        });
                    }
                }
            }
        }

        return (saisons, cultures, etapes, conseils);
    }

    public static string IdSaison(string zone, string saison) => $"{zone}|{saison}";

    // Même identifiant que celui utilisé à l'enregistrement d'une étape saisie à la main.
    public static string IdEtape(string cultureId, TypeEtape phase, string zone, string saison) =>
        $"{cultureId}|{(int)phase}|{zone}|{saison}";

    public static string TitreDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => AppResources.Charte_BadgeEntretien,
        TypeEtape.Recolte => AppResources.Charte_BadgeRecolte,
        _ => AppResources.Charte_BadgeSemis,
    };

    private static CategorieCulture LireCategorie(string cle) => cle switch
    {
        "cereale" => CategorieCulture.Cereale,
        "tubercule" => CategorieCulture.Tubercule,
        "legume" => CategorieCulture.Legume,
        "rente" => CategorieCulture.Rente,
        _ => throw new InvalidDataException($"Catégorie inconnue : {cle}"),
    };

    private static TypeEtape LirePhase(string cle) => cle switch
    {
        "semis" => TypeEtape.Semis,
        "entretien" => TypeEtape.Entretien,
        "recolte" => TypeEtape.Recolte,
        _ => throw new InvalidDataException($"Phase inconnue : {cle}"),
    };

}
