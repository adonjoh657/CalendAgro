using CalendAgro.Views;

namespace CalendAgro
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Pages de test : routes hors onglets.
            Routing.RegisterRoute("charte", typeof(ChartePage));
            Routing.RegisterRoute("composants", typeof(ComposantsPage));
            Routing.RegisterRoute("connexion", typeof(ConnexionPage));
            Routing.RegisterRoute("inscription", typeof(InscriptionPage));
            Routing.RegisterRoute("mot-de-passe-oublie", typeof(MotDePasseOubliePage));
            Routing.RegisterRoute("fiche-culture", typeof(FicheCulturePage));
            Routing.RegisterRoute("detail-tache", typeof(DetailTachePage));
            Routing.RegisterRoute("detail-etape", typeof(DetailEtapePage));
            Routing.RegisterRoute("ajout-parcelle", typeof(AjoutParcellePage));
        }
    }
}
