using CalendAgro.Models;

namespace CalendAgro.Services;

// Exemples non officiels (Mireille, 3 travaux, 3 parcelles).
public sealed class FakeAccueilService : IAccueilService
{
    private static readonly ConseilBloc[] ConseilsTerrain =
    [
        new("Prendre le temps d'observer", "Regardez l'état des feuilles et l'humidité apparente du sol sur plusieurs zones."),
        new("Garder une trace", "Notez les différences observées pour en parler lors d'un conseil local."),
        new("Adapter le travail", "Tenez compte des pluies récentes et des conditions de votre parcelle."),
    ];

    public async Task<DetailTravail?> GetDetailAsync(string travailId)
    {
        await Task.Delay(200);
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        return travailId switch
        {
            "t1" => new DetailTravail
            {
                Id = "t1",
                Titre = "Observer les jeunes plants",
                Accroche = "Un temps d'observation pour votre maïs.",
                Phase = TypeEtape.Entretien,
                DatePrevue = aujourdHui,
                ParcelleNom = "Parcelle du bas",
                CultureNom = "Maïs",
                SurfaceHa = 0.30,
                Conseils = ConseilsTerrain,
            },
            "t2" => new DetailTravail
            {
                Id = "t2",
                Titre = "Préparer le semis",
                Accroche = "Un temps de préparation pour votre haricot.",
                Phase = TypeEtape.Semis,
                DatePrevue = aujourdHui.AddDays(2),
                ParcelleNom = "Parcelle du puits",
                CultureNom = "Haricot",
                SurfaceHa = 0.15,
                Conseils = ConseilsTerrain,
            },
            "t3" => new DetailTravail
            {
                Id = "t3",
                Titre = "Vérifier la maturité",
                Accroche = "Un temps de contrôle pour votre manioc.",
                Phase = TypeEtape.Recolte,
                DatePrevue = aujourdHui.AddDays(6),
                ParcelleNom = "Parcelle de la maison",
                CultureNom = "Manioc",
                SurfaceHa = 0.40,
                Conseils = ConseilsTerrain,
            },
            _ => null,
        };
    }

    public Task MarquerTermineeAsync(string travailId)
    {
        return Task.Delay(300);
    }

    public async Task<AccueilDonnees> GetAccueilAsync()
    {
        await Task.Delay(400);
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        return new AccueilDonnees(
            "Mireille",
            3,
            3,
            "Maïs · Manioc · Haricot",
            0.85,
            [
                new Travail { Id = "t1", Titre = "Observer les jeunes plants", ParcelleNom = "Parcelle du bas", CultureNom = "Maïs", Date = aujourdHui, Phase = TypeEtape.Entretien },
                new Travail { Id = "t2", Titre = "Préparer le semis", ParcelleNom = "Parcelle du puits", CultureNom = "Haricot", Date = aujourdHui.AddDays(2), Phase = TypeEtape.Semis },
                new Travail { Id = "t3", Titre = "Vérifier la maturité", ParcelleNom = "Parcelle de la maison", CultureNom = "Manioc", Date = aujourdHui.AddDays(6), Phase = TypeEtape.Recolte },
            ]);
    }
}
