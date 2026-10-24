using CalendAgro.Models;

namespace CalendAgro.Services;

// Occurrences d'un rappel personnel : une seule fois, chaque jour, chaque
// semaine (même jour) ou chaque mois (même quantième, ramené au dernier jour
// des mois plus courts).
public static class RappelOccurrences
{
    public static bool TombeLe(RappelPersonnel rappel, DateOnly date)
    {
        if (date < rappel.DateDebut)
        {
            return false;
        }

        return rappel.Frequence switch
        {
            FrequenceRappel.UneFois => date == rappel.DateDebut,
            FrequenceRappel.Quotidienne => true,
            FrequenceRappel.Hebdomadaire => date.DayOfWeek == rappel.DateDebut.DayOfWeek,
            FrequenceRappel.Mensuelle => date.Day == JourDuMois(rappel.DateDebut, date.Year, date.Month),
            _ => false,
        };
    }

    // Première échéance strictement future, ou null si le rappel unique est passé.
    public static DateTime? ProchaineEcheance(RappelPersonnel rappel, DateTime maintenant)
    {
        var date = DateOnly.FromDateTime(maintenant);
        if (rappel.Frequence == FrequenceRappel.UneFois)
        {
            var unique = rappel.DateDebut.ToDateTime(rappel.Heure);
            return unique > maintenant ? unique : null;
        }

        // Une occurrence mensuelle tombe dans les 31 prochains jours ; la borne
        // large évite toute boucle infinie si les données sont incohérentes.
        for (var i = 0; i < 400; i++)
        {
            if (TombeLe(rappel, date))
            {
                var echeance = date.ToDateTime(rappel.Heure);
                if (echeance > maintenant)
                {
                    return echeance;
                }
            }

            date = date.AddDays(1);
        }

        return null;
    }

    private static int JourDuMois(DateOnly date, int annee, int mois) =>
        Math.Min(date.Day, DateTime.DaysInMonth(annee, mois));
}
