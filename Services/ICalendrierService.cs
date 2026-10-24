using CalendAgro.Models;

namespace CalendAgro.Services;

// Travaux d'une parcelle, calculés à partir des étapes de référence
// (culture + zone + saison). Aucune date n'est codée en dur.
public interface ICalendrierService
{
    Task<IReadOnlyList<Travail>> GetTravauxAsync(Parcelle parcelle, IReadOnlySet<TypeEtape> phasesTerminees);

    // Périodes complètes de la campagne : semis, entretien et récolte, même passés.
    Task<IReadOnlyList<PeriodeTravail>> GetPeriodesAsync(Parcelle parcelle);

    Task<DateOnly?> GetDatePhaseAsync(Parcelle parcelle, TypeEtape phase);
}
