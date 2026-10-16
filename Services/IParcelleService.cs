using CalendAgro.Models;

namespace CalendAgro.Services;

// Parcelles de l'utilisateur : liste en mémoire, SQLite plus tard.
public interface IParcelleService
{
    Task<IReadOnlyList<Parcelle>> GetParcellesAsync();

    Task<Parcelle?> GetParcelleAsync(string parcelleId);

    Task<Parcelle> AjouterAsync(string nom, string cultureId, double? surfaceHa, string region, DateOnly dateSemis);

    Task SupprimerAsync(string parcelleId);
}
