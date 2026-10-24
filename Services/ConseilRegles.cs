namespace CalendAgro.Services;

// Règles de saisie d'un conseil : court, 12 mots au maximum (cahier des charges, EF-04).
public static class ConseilRegles
{
    public const int MotsMax = 12;

    public static int CompterMots(string? texte) =>
        string.IsNullOrWhiteSpace(texte)
            ? 0
            : texte.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    public static bool EstValide(string? texte) => CompterMots(texte) <= MotsMax;
}
