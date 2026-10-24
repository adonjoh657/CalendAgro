using CalendAgro.Models;

namespace CalendAgro.Services;

// Rappels personnels saisis dans le calendrier : stockage local, aucun réseau.
public interface IRappelPersonnelService
{
    Task<IReadOnlyList<RappelPersonnel>> GetRappelsAsync();

    Task<RappelPersonnel> AjouterAsync(
        string titre,
        DateOnly dateDebut,
        TimeOnly heure,
        FrequenceRappel frequence);

    Task SetActifAsync(string rappelId, bool actif);

    Task SupprimerAsync(string rappelId);
}
