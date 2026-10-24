using CalendAgro.Models;

namespace CalendAgro.Services;

// Profil local de l'utilisateur : lecture, enregistrement à la connexion
// et déconnexion. Aucune donnée ne quitte l'appareil.
public interface IProfilService
{
    Task<ProfilUtilisateur?> GetProfilAsync();

    Task EnregistrerAsync(string nom, string identifiant, string telephone);

    Task DeconnecterAsync();
}
