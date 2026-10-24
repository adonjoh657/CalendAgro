using CalendAgro.Models;

namespace CalendAgro.Services;

// Données d'accueil : chiffres, travaux à venir et résumé des parcelles.
// Tout est calculé à partir des parcelles enregistrées, rien n'est simulé.
public sealed record AccueilDonnees(
    int NbTravaux,
    int NbParcelles,
    string CulturesResume,
    double SurfaceTotaleHa,
    IReadOnlyList<Travail> Travaux);

public interface IAccueilService
{
    Task<AccueilDonnees> GetAccueilAsync();

    // Toutes les périodes (semis, entretien, récolte) de toutes les parcelles, passées ou non.
    Task<IReadOnlyList<PeriodeTravail>> GetCalendrierAsync();

    Task<DetailTravail?> GetDetailAsync(string travailId);

    Task MarquerTermineeAsync(string travailId);
}
