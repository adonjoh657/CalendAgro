using CalendAgro.Models;

namespace CalendAgro.Services;

// Rappels de saison de l'utilisateur : état actif stocké en SQLite,
// dates recalculées hors ligne depuis les parcelles et le calendrier.
public interface IRappelService
{
    Task<IReadOnlyList<Rappel>> GetRappelsAsync();

    Task SetActifAsync(string rappelId, bool actif);
}
