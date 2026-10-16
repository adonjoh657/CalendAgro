namespace CalendAgro.Controls;

// Badge d'étape : pastille remplie couleur de phase + libellé dedans.
// Blanc sur Semis/Entretien, sombre sur Récolte (contraste, maquette Figma).
public partial class StepBadge : ContentView
{
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(StepKind), typeof(StepBadge), StepKind.Semis,
        propertyChanged: (b, _, _) => ((StepBadge)b).Refresh());

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(StepBadge), string.Empty,
        propertyChanged: (b, _, n) => ((StepBadge)b).TextLabel.Text = (string)n);

    public StepKind Kind
    {
        get => (StepKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public StepBadge()
    {
        InitializeComponent();
        Refresh();
    }

    private void Refresh()
    {
        var resources = Application.Current?.Resources;
        var (colorKey, onKey) = Kind switch
        {
            StepKind.Entretien => ("Entretien", "OnPrimary"),
            StepKind.Recolte => ("Recolte", "OnAccent"),
            _ => ("Semis", "OnPrimary"),
        };
        if (resources?.TryGetValue(colorKey, out var color) == true && color is Color badge)
        {
            Pill.BackgroundColor = badge;
        }
        if (resources?.TryGetValue(onKey, out var on) == true && on is Color onColor)
        {
            TextLabel.TextColor = onColor;
        }
    }
}
