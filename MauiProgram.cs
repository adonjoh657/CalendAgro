using CalendAgro.Services;
using CalendAgro.ViewModels;
using CalendAgro.Views;
using Microsoft.Extensions.Logging;

namespace CalendAgro
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Inter-Variable.ttf", "Inter");
                });

            builder.Services.AddSingleton<ITabNavigationService, TabNavigationService>();
            builder.Services.AddSingleton<ISessionService, FakeSessionService>();
            builder.Services.AddSingleton<IAuthService, FakeAuthService>();
            builder.Services.AddSingleton<ICultureService, FakeCultureService>();
            builder.Services.AddSingleton<IAccueilService, FakeAccueilService>();
            builder.Services.AddSingleton<IParcelleService, FakeParcelleService>();
            builder.Services.AddTransient<ConnexionViewModel>();
            builder.Services.AddTransient<ConnexionPage>();
            builder.Services.AddTransient<InscriptionViewModel>();
            builder.Services.AddTransient<InscriptionPage>();
            builder.Services.AddTransient<MotDePasseOublieViewModel>();
            builder.Services.AddTransient<MotDePasseOubliePage>();
            builder.Services.AddTransient<FicheCultureViewModel>();
            builder.Services.AddTransient<FicheCulturePage>();
            builder.Services.AddTransient<DetailTacheViewModel>();
            builder.Services.AddTransient<DetailTachePage>();
            builder.Services.AddTransient<DetailEtapeViewModel>();
            builder.Services.AddTransient<DetailEtapePage>();
            builder.Services.AddTransient<AjoutParcelleViewModel>();
            builder.Services.AddTransient<AjoutParcellePage>();
            builder.Services.AddTransient<FicheParcelleViewModel>();
            builder.Services.AddTransient<FicheParcellePage>();
            builder.Services.AddTransient<DemarrageViewModel>();
            builder.Services.AddTransient<DemarragePage>();
            builder.Services.AddTransient<CharteViewModel>();
            builder.Services.AddTransient<ChartePage>();
            builder.Services.AddTransient<AccueilViewModel>();
            builder.Services.AddTransient<AccueilPage>();
            builder.Services.AddTransient<CulturesViewModel>();
            builder.Services.AddTransient<CulturesPage>();
            builder.Services.AddTransient<CalendrierViewModel>();
            builder.Services.AddTransient<CalendrierPage>();
            builder.Services.AddTransient<ParcellesViewModel>();
            builder.Services.AddTransient<ParcellesPage>();
            builder.Services.AddTransient<ComposantsViewModel>();
            builder.Services.AddTransient<ComposantsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
