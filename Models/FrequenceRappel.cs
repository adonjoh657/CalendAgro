using CalendAgro.Resources.Strings;

namespace CalendAgro.Models;

// Rythme d'un rappel personnel créé depuis le calendrier.
public enum FrequenceRappel
{
    UneFois,
    Quotidienne,
    Hebdomadaire,
    Mensuelle
}

internal static class FrequenceRappelLabels
{
    internal static string Libelle(FrequenceRappel frequence) => frequence switch
    {
        FrequenceRappel.Quotidienne => AppResources.Rappel_FreqQuotidienne,
        FrequenceRappel.Hebdomadaire => AppResources.Rappel_FreqHebdomadaire,
        FrequenceRappel.Mensuelle => AppResources.Rappel_FreqMensuelle,
        _ => AppResources.Rappel_FreqUneFois,
    };
}
