using CalendAgro.Models;

namespace CalendAgro.Services;

// 3 parcelles de Mireille, cohérentes avec l'accueil. Ajouts conservés
// en mémoire pendant la session (SQLite à l'implémentation réelle).
public sealed class FakeParcelleService : IParcelleService
{
    private readonly ICultureService _cultures;
    private readonly List<Parcelle> _parcelles = [];
    private bool _initialise;

    public FakeParcelleService(ICultureService cultures) => _cultures = cultures;

    public async Task<IReadOnlyList<Parcelle>> GetParcellesAsync()
    {
        await Task.Delay(400);
        if (!_initialise)
        {
            _initialise = true;
            var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
            _parcelles.AddRange(
            [
                new Parcelle { Id = "p1", Nom = "Parcelle du bas", CultureId = "mais", CultureNom = "Maïs", SurfaceHa = 0.30, Region = "Centre", DateSemis = aujourdHui.AddDays(-30), ProchainTravail = new Travail { Id = "t1", Titre = "Observer les jeunes plants", ParcelleNom = "Parcelle du bas", CultureNom = "Maïs", Date = aujourdHui, Phase = TypeEtape.Entretien } },
                new Parcelle { Id = "p2", Nom = "Parcelle de la maison", CultureId = "manioc", CultureNom = "Manioc", SurfaceHa = 0.40, Region = "Centre", DateSemis = aujourdHui.AddDays(-200), ProchainTravail = new Travail { Id = "t3", Titre = "Vérifier la maturité", ParcelleNom = "Parcelle de la maison", CultureNom = "Manioc", Date = aujourdHui.AddDays(6), Phase = TypeEtape.Recolte } },
                new Parcelle { Id = "p3", Nom = "Parcelle du puits", CultureId = "haricot", CultureNom = "Haricot", SurfaceHa = 0.15, Region = "Centre", DateSemis = aujourdHui.AddDays(-10), ProchainTravail = new Travail { Id = "t2", Titre = "Préparer le semis", ParcelleNom = "Parcelle du puits", CultureNom = "Haricot", Date = aujourdHui.AddDays(2), Phase = TypeEtape.Semis } },
            ]);
        }
        return _parcelles.ToArray();
    }

    public async Task<Parcelle> AjouterAsync(string nom, string cultureId, double? surfaceHa, string region, DateOnly dateSemis)
    {
        await Task.Delay(400);
        var culture = await _cultures.GetCultureAsync(cultureId);
        var parcelle = new Parcelle
        {
            Id = Guid.NewGuid().ToString("N"),
            Nom = nom.Trim(),
            CultureId = cultureId,
            CultureNom = culture?.Nom ?? cultureId,
            SurfaceHa = surfaceHa ?? 0,
            Region = region.Trim(),
            DateSemis = dateSemis,
            ProchainTravail = null,
        };
        _parcelles.Add(parcelle);
        return parcelle;
    }
}
