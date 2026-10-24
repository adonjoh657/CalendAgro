using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using Plugin.LocalNotification.Core.Models.AndroidOption;

namespace CalendAgro.Services;

// Notifications locales des rappels personnels : aucune ressource réseau.
public interface IRappelNotificationService
{
    Task DemanderPermissionAsync();

    Task PlanifierAsync(RappelPersonnel rappel);

    void Annuler(string rappelId);
}

// Une seule demande de notification par rappel : le plugin gère la répétition
// quotidienne, hebdomadaire ou mensuelle. La planification ne doit jamais
// bloquer l'enregistrement local du rappel, d'où les échecs silencieux.
public sealed class RappelNotificationService : IRappelNotificationService
{
    // Canal neuf : Android fige le son d'un canal après sa création.
    public const string Canal = "calendagro-rappels-alarme";

    // Sonnerie d'alarme du système, pour distinguer un rappel d'une notification.
    public const string Sonnerie = "content://settings/system/alarm_alert";

    public async Task DemanderPermissionAsync()
    {
        try
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
        catch
        {
            // Permission indisponible ou refusée : le rappel reste utilisable dans l'app.
        }
    }

    public async Task PlanifierAsync(RappelPersonnel rappel)
    {
        if (!rappel.Actif)
        {
            Annuler(rappel.Id);
            return;
        }

        var echeance = RappelOccurrences.ProchaineEcheance(rappel, DateTime.Now);
        if (echeance is null)
        {
            Annuler(rappel.Id);
            return;
        }

        try
        {
            var requete = new NotificationRequest
            {
                NotificationId = Identifiant(rappel.Id),
                Title = rappel.Titre,
                Description = AppResources.Rappel_NotificationDescription,
                Android = new AndroidOptions
                {
                    ChannelId = Canal,
                    Priority = AndroidPriority.High,
                    AudioAttributeUsage = AndroidAudioAttributeUsage.Alarm,
                },
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = echeance.Value,
                    RepeatType = RepeatDe(rappel.Frequence),
                },
            };
#if ANDROID
            // Avant Android 8, le son se règle sur la requête et non sur le canal.
            requete.Sound = Sonnerie;
#endif
            await LocalNotificationCenter.Current.Show(requete);
        }
        catch
        {
            // Même raison : le rappel est déjà enregistré localement.
        }
    }

    public void Annuler(string rappelId) => LocalNotificationCenter.Current.Cancel(Identifiant(rappelId));

    private static NotificationRepeat RepeatDe(FrequenceRappel frequence) => frequence switch
    {
        FrequenceRappel.Quotidienne => NotificationRepeat.Daily,
        FrequenceRappel.Hebdomadaire => NotificationRepeat.Weekly,
        FrequenceRappel.Mensuelle => NotificationRepeat.Monthly,
        _ => NotificationRepeat.No,
    };

    // FNV-1a : identifiant stable entre les sessions, contrairement à string.GetHashCode.
    private static int Identifiant(string rappelId)
    {
        unchecked
        {
            var hash = 2166136261u;
            foreach (var caractere in rappelId)
            {
                hash ^= caractere;
                hash *= 16777619u;
            }

            return (int)(hash & 0x7FFFFFFF);
        }
    }
}
