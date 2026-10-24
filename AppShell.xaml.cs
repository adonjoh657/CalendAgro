using CalendAgro.Services;
using CalendAgro.Views;

namespace CalendAgro
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("connexion", typeof(ConnexionPage));
            Routing.RegisterRoute("inscription", typeof(InscriptionPage));
            Routing.RegisterRoute("mot-de-passe-oublie", typeof(MotDePasseOubliePage));
            Routing.RegisterRoute("fiche-culture", typeof(FicheCulturePage));
            Routing.RegisterRoute("detail-tache", typeof(DetailTachePage));
            Routing.RegisterRoute("detail-etape", typeof(DetailEtapePage));
            Routing.RegisterRoute("edition-culture", typeof(EditionCulturePage));
            Routing.RegisterRoute("edition-etape", typeof(EditionEtapePage));
            Routing.RegisterRoute("ajout-parcelle", typeof(AjoutParcellePage));
            Routing.RegisterRoute("fiche-parcelle", typeof(FicheParcellePage));
            Routing.RegisterRoute("rappels", typeof(RappelsPage));
            Routing.RegisterRoute("profil", typeof(ProfilPage));
        }

        // Sur un onglet racine, le retour système ramène à l'accueil au lieu de
        // fermer l'application. Sur une page empilée, le comportement Shell reste.
        protected override bool OnBackButtonPressed()
        {
            var route = Shell.Current?.CurrentState?.Location.OriginalString.Split('/').LastOrDefault();
            if (route is "cultures" or "calendrier" or "parcelles")
            {
                _ = DefaultServices.Navigation.GoHomeAsync();
                return true;
            }

            return base.OnBackButtonPressed();
        }
    }
}
