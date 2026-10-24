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

    internal static async Task SlideOutAsync(VisualElement? content, int step)
    {
        if (content is null || step == 0)
        {
            return;
        }

        await SlideXAsync(content, -SlideOffset * step, SlideOutDurationMs, Easing.CubicIn);
    }

    internal static async Task SlideInAsync(VisualElement? content, int step)
    {
        if (content is null)
        {
            return;
        }

        if (step == 0)
        {
            // Ouverture par onglet : instantané, état neutre garanti.
            content.TranslationX = 0;
            return;
        }

        content.TranslationX = SlideOffset * step;
        await SlideXAsync(content, 0, SlideInDurationMs, Easing.CubicOut);
    }

    // TranslationX seule : le fondu d'entrée (PageTransition) anime TranslationY,
    // une animation sur les deux axes écraserait l'autre.
    private static Task SlideXAsync(VisualElement content, double to, uint duration, Easing easing)
    {
        var animation = new Animation(value => content.TranslationX = value, content.TranslationX, to, easing);
        animation.Commit(content, "TabSlideX", 16, duration, finished: (_, _) => content.TranslationX = to);
        return Task.Delay((int)duration);
    }
}
