using CalendAgro.Data;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CalendAgro.ViewModels;
using CalendAgro.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models.AndroidOption;

namespace CalendAgro
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification(config => config.AddAndroid(android => android.AddChannel(
                    new AndroidNotificationChannelRequest
                    {
                        Id = RappelNotificationService.Canal,
                        Name = AppResources.Rappels_CanalNom,
                        Description = AppResources.Rappels_CanalDescription,
                        Importance = AndroidImportance.High,
                        Sound = RappelNotificationService.Sonnerie,
                        EnableSound = true,
                        EnableVibration = true,
                    })))
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Inter-Variable.ttf", "Inter");
                });

            builder.Services.AddSingleton<AppDatabase>();
            builder.Services.AddSingleton<ITabNavigationService, TabNavigationService>();
            builder.Services.AddSingleton<IProfilService, SQLiteProfilService>();
            builder.Services.AddSingleton<ISessionService, FakeSessionService>();
            builder.Services.AddSingleton<IAuthService, FakeAuthService>();
            builder.Services.AddSingleton<ICultureService, SQLiteCultureService>();
            builder.Services.AddSingleton<ICalendrierService, CalendrierService>();
            builder.Services.AddSingleton<IParcelleService, SQLiteParcelleService>();
            builder.Services.AddSingleton<IRappelService, SQLiteRappelService>();
            builder.Services.AddSingleton<IRappelPersonnelService, SQLiteRappelPersonnelService>();
            builder.Services.AddSingleton<IRappelNotificationService, RappelNotificationService>();
            builder.Services.AddSingleton<IAccueilService, AccueilService>();
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
            builder.Services.AddTransient<DetailJourViewModel>();
            builder.Services.AddTransient<DetailJourPage>();
            builder.Services.AddTransient<DetailEtapeViewModel>();
            builder.Services.AddTransient<DetailEtapePage>();
            builder.Services.AddTransient<EditionCultureViewModel>();
            builder.Services.AddTransient<EditionCulturePage>();
            builder.Services.AddTransient<EditionEtapeViewModel>();
            builder.Services.AddTransient<EditionEtapePage>();
            builder.Services.AddTransient<AjoutParcelleViewModel>();
            builder.Services.AddTransient<AjoutParcellePage>();
            builder.Services.AddTransient<FicheParcelleViewModel>();
            builder.Services.AddTransient<FicheParcellePage>();
            builder.Services.AddTransient<DemarrageViewModel>();
            builder.Services.AddTransient<DemarragePage>();
            builder.Services.AddTransient<AccueilViewModel>();
            builder.Services.AddTransient<AccueilPage>();
            builder.Services.AddTransient<CulturesViewModel>();
            builder.Services.AddTransient<CulturesPage>();
            builder.Services.AddTransient<CalendrierViewModel>();
            builder.Services.AddTransient<CalendrierPage>();
            builder.Services.AddTransient<ParcellesViewModel>();
            builder.Services.AddTransient<ParcellesPage>();
            builder.Services.AddTransient<RappelsViewModel>();
            builder.Services.AddTransient<RappelsPage>();
            builder.Services.AddTransient<EditionRappelViewModel>();
            builder.Services.AddTransient<EditionRappelPage>();
            builder.Services.AddTransient<ProfilViewModel>();
            builder.Services.AddTransient<ProfilPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
