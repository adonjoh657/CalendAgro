using CalendAgro.Data;

namespace CalendAgro.Services;

// Graphe de services par défaut, partagé par les constructeurs de repli des
// ViewModels (créés hors injection de dépendances, ex. DataTemplate de Shell).
// L'ordre des champs compte : chaque instance est construite avec les précédentes.
internal static class DefaultServices
{
    private static readonly AppDatabase BaseDeDonnees = new();
    private static readonly ICultureService CulturesUniques = new SQLiteCultureService(BaseDeDonnees);
    private static readonly ITabNavigationService NavigationUnique = new TabNavigationService();
    private static readonly IProfilService ProfilUnique = new SQLiteProfilService(BaseDeDonnees);
    private static readonly ISessionService SessionUnique = new FakeSessionService(ProfilUnique);
    private static readonly IAuthService AuthentificationUnique = new FakeAuthService(ProfilUnique);
    private static readonly ICalendrierService CalendrierUnique = new CalendrierService(CulturesUniques);
    private static readonly IParcelleService ParcellesUniques =
        new SQLiteParcelleService(BaseDeDonnees, CalendrierUnique);
    private static readonly IRappelService RappelsUniques =
        new SQLiteRappelService(BaseDeDonnees, ParcellesUniques, CalendrierUnique);
    private static readonly IAccueilService AccueilUnique =
        new AccueilService(ParcellesUniques, CalendrierUnique, CulturesUniques);

    internal static ICultureService Cultures => CulturesUniques;

    internal static ITabNavigationService Navigation => NavigationUnique;

    internal static ISessionService Session => SessionUnique;

    internal static IAuthService Authentification => AuthentificationUnique;

    internal static IProfilService Profil => ProfilUnique;

    internal static IParcelleService Parcelles => ParcellesUniques;

    internal static IRappelService Rappels => RappelsUniques;

    internal static IAccueilService Accueil => AccueilUnique;
}
