using CalendAgro.Models;

namespace CalendAgro.Services;

// Identifiant stable d'un travail : parcelle + phase. Le détail d'une tâche est
// reconstruit à partir de cet identifiant, rien n'est stocké en double.
internal static class TravailId
{
    private const char Separateur = '|';

    internal static string Creer(string parcelleId, TypeEtape phase) =>
        $"{parcelleId}{Separateur}{phase}";

    internal static bool TryLire(string? valeur, out string parcelleId, out TypeEtape phase)
    {
        parcelleId = string.Empty;
        phase = default;
        if (string.IsNullOrEmpty(valeur))
        {
            return false;
        }

        var index = valeur.LastIndexOf(Separateur);
        if (index <= 0 || index == valeur.Length - 1)
        {
            return false;
        }

        if (!Enum.TryParse(valeur[(index + 1)..], out phase))
        {
            return false;
        }

        parcelleId = valeur[..index];
        return true;
    }
}
