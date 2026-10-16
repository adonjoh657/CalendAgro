namespace CalendAgro.Controls;

// Carte Surface, coins 12. Titre masqué quand vide.
public partial class AppCard : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(AppCard), string.Empty,
        propertyChanged: (b, _, n) => ((AppCard)b).Refresh((string)n));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public AppCard()
    {
        InitializeComponent();
        Refresh(Title);
    }

    private void Refresh(string title)
    {
        TitleLabel.Text = title;
        TitleLabel.IsVisible = !string.IsNullOrWhiteSpace(title);
    }
}
