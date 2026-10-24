using CalendAgro.Models;

namespace CalendAgro.Services;

// Parcelles de l'utilisateur : stockées en SQLite, source principale (AGENTS §10).
public interface IParcelleService
{
    Task<IReadOnlyList<Parcelle>> GetParcellesAsync();

    Task<Parcelle?> GetParcelleAsync(string parcelleId);

    Task<Parcelle> AjouterAsync(string nom, string cultureNom, double? surfaceHa, string region, DateOnly dateSemis);

    Task SupprimerAsync(string parcelleId);

    Task<IReadOnlySet<TypeEtape>> GetPhasesTermineesAsync(string parcelleId);

    Task MarquerTermineeAsync(string parcelleId, TypeEtape phase, DateOnly datePrevue);
}
