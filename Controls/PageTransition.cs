namespace CalendAgro.Controls;

// Effet d'entrée commun à toutes les pages : fondu + légère montée.
// Demandé explicitement pour adoucir les changements d'écran ; il reste
// court et n'anime que l'opacité et TranslationY, pour ne pas gêner le
// glissement horizontal des onglets (PageSlideAnimator).
public static class PageTransition
{
    private const double EnterOffset = 8;
    private const uint EnterDurationMs = 180;

    public static readonly BindableProperty IsEnabledProperty = BindableProperty.CreateAttached(
        "IsEnabled",
        typeof(bool),
        typeof(PageTransition),
        false,
        propertyChanged: OnIsEnabledChanged);

    public static bool GetIsEnabled(BindableObject bindable) => (bool)bindable.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(BindableObject bindable, bool value) => bindable.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not ContentPage page)
        {
            return;
        }

        page.Appearing -= OnPageAppearing;
        if (newValue is true)
        {
            page.Appearing += OnPageAppearing;
        }
    }

    private static void OnPageAppearing(object? sender, EventArgs e)
    {
        if (sender is not ContentPage { Content: VisualElement content })
        {
            return;
        }

        content.Opacity = 0;
        content.TranslationY = EnterOffset;

        var rise = new Animation(
            value => content.TranslationY = value,
            EnterOffset,
            0,
            Easing.CubicOut);
        rise.Commit(content, "PageEnter", 16, EnterDurationMs, finished: (_, _) => content.TranslationY = 0);

        _ = content.FadeToAsync(1, EnterDurationMs, Easing.CubicOut);
    }
}
