using System.Globalization;
using System.Text;

namespace CalendAgro.Services;

// Identifiant stable d'une culture, dérivé de son nom (sans accents, minuscules,
// tirets). Parcelles et cultures utilisent la même règle pour rester rattachées.
internal static class CultureId
{
    internal static string Creer(string nom)
    {
        var decompose = nom.Trim().Normalize(NormalizationForm.FormD);
        var texte = new StringBuilder(decompose.Length);
        foreach (var caractere in decompose)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(caractere))
            {
                texte.Append(char.ToLowerInvariant(caractere));
            }
            else if (texte.Length > 0 && texte[^1] != '-')
            {
                texte.Append('-');
            }
        }

        var identifiant = texte.ToString().Trim('-');
        return identifiant.Length > 0 ? identifiant : nom.Trim().ToLowerInvariant();
    }

    // Forme de comparaison pour la recherche : sans accents ni majuscules.
    internal static string Normaliser(string texte)
    {
        var decompose = texte.Trim().Normalize(NormalizationForm.FormD);
        var resultat = new StringBuilder(decompose.Length);
        foreach (var caractere in decompose)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                resultat.Append(char.ToLowerInvariant(caractere));
            }
        }

        return resultat.ToString();
    }
}
