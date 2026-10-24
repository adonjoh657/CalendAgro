using CalendAgro.Data;
using CalendAgro.Models;

namespace CalendAgro.Services;

// Profil en SQLite : source principale locale (AGENTS §10).
// La déconnexion marque la ligne supprimée sans effacer les parcelles.
public sealed class SQLiteProfilService : IProfilService
{
    private const string IdProfil = "profil";
    private const string LangueParDefaut = "fr";

    private readonly AppDatabase _donnees;

    public SQLiteProfilService(AppDatabase donnees) => _donnees = donnees;

    public async Task<ProfilUtilisateur?> GetProfilAsync()
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<ProfilRow>(IdProfil).ConfigureAwait(false);
        return ligne is null || ligne.IsDeleted
            ? null
            : new ProfilUtilisateur(ligne.Nom, ligne.Identifiant, ligne.Telephone);
    }

    public async Task EnregistrerAsync(string nom, string identifiant, string telephone)
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<ProfilRow>(IdProfil).ConfigureAwait(false)
            ?? new ProfilRow { Id = IdProfil };

        ligne.Nom = nom.Trim();
        ligne.Identifiant = identifiant.Trim();
        ligne.Telephone = telephone.Trim();
        ligne.Langue = LangueParDefaut;
        ligne.IsDeleted = false;
        ligne.UpdatedAt = DateTime.UtcNow.Ticks;
        await connexion.InsertOrReplaceAsync(ligne).ConfigureAwait(false);
    }

    public async Task DeconnecterAsync()
    {
        var connexion = await _donnees.GetConnexionAsync().ConfigureAwait(false);
        var ligne = await connexion.FindAsync<ProfilRow>(IdProfil).ConfigureAwait(false);
        if (ligne is null)
        {
            return;
        }

        ligne.IsDeleted = true;
        ligne.UpdatedAt = DateTime.UtcNow.Ticks;
        await connexion.UpdateAsync(ligne).ConfigureAwait(false);
    }
}
