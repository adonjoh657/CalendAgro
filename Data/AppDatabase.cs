using Microsoft.Maui.Storage;
using SQLite;
using SQLitePCL;

namespace CalendAgro.Data;

// Base SQLite locale : source principale des données utilisateur (AGENTS §10).
// Ouverte au premier accès et créée table par table, sans bloquer l'interface.
public sealed class AppDatabase
{
    private const string NomFichier = "calendagro.db3";

    private static bool _moteurInitialise;
    private readonly SemaphoreSlim _verrou = new(1, 1);
    private SQLiteAsyncConnection? _connexion;

    public async Task<SQLiteAsyncConnection> GetConnexionAsync()
    {
        if (_connexion is not null)
        {
            return _connexion;
        }

        await _verrou.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_connexion is null)
            {
                InitialiserMoteur();
                var chemin = Path.Combine(FileSystem.AppDataDirectory, NomFichier);
                var connexion = new SQLiteAsyncConnection(
                    chemin,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
                await connexion.CreateTableAsync<ParcelleRow>().ConfigureAwait(false);
                await connexion.CreateTableAsync<TravailTermineRow>().ConfigureAwait(false);
                await connexion.CreateTableAsync<RappelRow>().ConfigureAwait(false);
                await connexion.CreateTableAsync<RappelPersonnelRow>().ConfigureAwait(false);
                await connexion.CreateTableAsync<ProfilRow>().ConfigureAwait(false);
                await connexion.CreateTableAsync<CultureRow>().ConfigureAwait(false);
                await connexion.CreateTableAsync<EtapeRow>().ConfigureAwait(false);
                await connexion.CreateTableAsync<ConseilRow>().ConfigureAwait(false);
                await connexion.CreateTableAsync<SaisonRow>().ConfigureAwait(false);
                _connexion = connexion;
            }

            return _connexion;
        }
        finally
        {
            _verrou.Release();
        }
    }

    private static void InitialiserMoteur()
    {
        if (_moteurInitialise)
        {
            return;
        }

        Batteries_V2.Init();
        _moteurInitialise = true;
    }
}
