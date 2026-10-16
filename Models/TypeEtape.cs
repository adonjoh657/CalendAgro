using CalendAgro.Resources.Strings;

namespace CalendAgro.Models;

// Phase culturale : mêmes valeurs que le badge, sans dépendre des contrôles.
public enum TypeEtape
{
    Semis,
    Entretien,
    Recolte
}

internal static class TypeEtapeLabels
{
    internal static string Libelle(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => AppResources.Charte_BadgeEntretien,
        TypeEtape.Recolte => AppResources.Charte_BadgeRecolte,
        _ => AppResources.Charte_BadgeSemis,
    };
}
