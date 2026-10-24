namespace CalendAgro.Services;

// Navigation au doigt entre les 4 onglets, dans l'ordre du TabBar.
// Bloquée aux extrémités : aucun mouvement sur Accueil-droite ni Parcelles-gauche.
// Toute la navigation Shell reste hors ligne, aucun appel réseau.
public interface ITabNavigationService
{
    IReadOnlyList<string> TabRoutes { get; }

    Task GoNextAsync();

    Task GoPreviousAsync();

    // Retour direct à l'accueil depuis un autre onglet.
    Task GoHomeAsync();

    // Direction du dernier déplacement : +1 (gauche), -1 (droite), 0 (aucun).
    // Remet à zéro après lecture, pour un seul effet par déplacement.
    int ConsumeLastStep();
}
