using CalendAgro.Models;

namespace CalendAgro.Services;

// Données d'accueil : salutation, chiffres, travaux, résumé parcelles.
// JSON/SQLite plus tard ; factice pour la phase B.
public sealed record AccueilDonnees(
    string UserName,
    int NbTravaux,
    int NbParcelles,
    string CulturesResume,
    double SurfaceTotaleHa,
    IReadOnlyList<Travail> Travaux);

public interface IAccueilService
{
    Task<AccueilDonnees> GetAccueilAsync();

    Task<DetailTravail?> GetDetailAsync(string travailId);

    Task MarquerTermineeAsync(string travailId);
}
