using CalendAgro.Models;

namespace CalendAgro.Services;

// Travaux d'une parcelle, calculés à partir des étapes de référence
// (culture + zone + saison). Aucune date n'est codée en dur.
public interface ICalendrierService
{
    Task<IReadOnlyList<Travail>> GetTravauxAsync(Parcelle parcelle, IReadOnlySet<TypeEtape> phasesTerminees);

    Task<DateOnly?> GetDatePhaseAsync(Parcelle parcelle, TypeEtape phase);
}
