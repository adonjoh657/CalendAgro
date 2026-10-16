using System.Windows.Input;

namespace CalendAgro.Controls;

// Bouton 56 px de la charte. Le style (Primary / Accent) vient des ressources.
public partial class AppButton : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(AppButton), string.Empty,
        propertyChanged: (b, _, n) => ((AppButton)b).InnerButton.Text = (string)n);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(AppButton),
        propertyChanged: (b, _, n) => ((AppButton)b).InnerButton.Command = (ICommand?)n);

    public static readonly BindableProperty VariantProperty = BindableProperty.Create(
        nameof(Variant), typeof(ButtonVariant), typeof(AppButton), ButtonVariant.Primary,
        propertyChanged: (b, _, _) => ((AppButton)b).Refresh());

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public AppButton()
    {
        InitializeComponent();
        Refresh();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName)
        {
            InnerButton.IsEnabled = IsEnabled;
        }
    }

    private void Refresh()
    {
        if (Application.Current?.Resources is not ResourceDictionary resources)
        {
            return;
        }
        var key = Variant switch
        {
            ButtonVariant.Accent => "AccentButton",
            ButtonVariant.Secondary => "SecondaryButton",
            _ => null,
        };
        if (key is not null
            && resources.TryGetValue(key, out var named)
            && named is Style namedStyle)
        {
            InnerButton.Style = namedStyle;
        }
        else
        {
            // Style implicite du bouton primaire.
            InnerButton.ClearValue(Button.StyleProperty);
        }
    }
}
