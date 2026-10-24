using Microsoft.Extensions.DependencyInjection;

namespace CalendAgro
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Thème clair imposé : garantit la lisibilité (AGENTS.md §8) et évite
            // les contrôles natifs en texte clair sur fond blanc si l'OS est en sombre.
            UserAppTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}