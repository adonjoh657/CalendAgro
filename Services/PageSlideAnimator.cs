namespace CalendAgro.Services;

// Effet de glissement entre onglets : la page sort vers le doigt puis
// la suivante entre en glissant, pour rendre le déplacement visible.
// Pur MAUI, sans dépendance, utilisable hors ligne.
internal static class PageSlideAnimator
{
    // Décalage en px (contenu 390 px de large : déplacement bien visible, texte net).
    private const int SlideOffset = 80;
    private const uint SlideOutDurationMs = 120;
    private const uint SlideInDurationMs = 200;

    internal static Task SlideOutAsync(VisualElement? content, int step)
    {
        if (content is null || step == 0)
        {
            return Task.CompletedTask;
        }

        return content.TranslateToAsync(-SlideOffset * step, 0, SlideOutDurationMs, Easing.CubicIn);
    }

    internal static Task SlideInAsync(VisualElement? content, int step)
    {
        if (content is null)
        {
            return Task.CompletedTask;
        }

        if (step == 0)
        {
            // Ouverture par onglet : instantané, état neutre garanti.
            content.TranslationX = 0;
            return Task.CompletedTask;
        }

        content.TranslationX = SlideOffset * step;
        return content.TranslateToAsync(0, 0, SlideInDurationMs, Easing.CubicOut);
    }
}
