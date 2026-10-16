using CalendAgro.Resources.Strings;

namespace CalendAgro.Controls;

// Champ de saisie 56 px : libellé, aide, erreur (croix) et succès (coche).
// Le bouton Afficher/Masquer apparaît pour les mots de passe.
public partial class ValidatedField : ContentView
{
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(ValidatedField), string.Empty,
        propertyChanged: (b, _, n) => ((ValidatedField)b).LabelLabel.Text = (string)n);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(ValidatedField), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(ValidatedField), string.Empty,
        propertyChanged: (b, _, n) => ((ValidatedField)b).Input.Placeholder = (string)n);

    public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(
        nameof(Keyboard), typeof(Keyboard), typeof(ValidatedField), Keyboard.Default,
        propertyChanged: (b, _, n) => ((ValidatedField)b).Input.Keyboard = (Keyboard)n);

    public static readonly BindableProperty IsPasswordFieldProperty = BindableProperty.Create(
        nameof(IsPasswordField), typeof(bool), typeof(ValidatedField), false,
        propertyChanged: (b, _, n) => ((ValidatedField)b).RefreshToggle((bool)n));

    public static readonly BindableProperty AideProperty = BindableProperty.Create(
        nameof(Aide), typeof(string), typeof(ValidatedField), string.Empty,
        propertyChanged: (b, _, n) => ((ValidatedField)b).RefreshAide((string)n));

    public static readonly BindableProperty ErreurProperty = BindableProperty.Create(
        nameof(Erreur), typeof(string), typeof(ValidatedField), string.Empty,
        propertyChanged: (b, _, n) => ((ValidatedField)b).RefreshErreur((string)n));

    public static readonly BindableProperty SuccesProperty = BindableProperty.Create(
        nameof(Succes), typeof(string), typeof(ValidatedField), string.Empty,
        propertyChanged: (b, _, n) => ((ValidatedField)b).RefreshSucces((string)n));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public bool IsPasswordField
    {
        get => (bool)GetValue(IsPasswordFieldProperty);
        set => SetValue(IsPasswordFieldProperty, value);
    }

    public string Aide
    {
        get => (string)GetValue(AideProperty);
        set => SetValue(AideProperty, value);
    }

    public string Erreur
    {
        get => (string)GetValue(ErreurProperty);
        set => SetValue(ErreurProperty, value);
    }

    public string Succes
    {
        get => (string)GetValue(SuccesProperty);
        set => SetValue(SuccesProperty, value);
    }

    public ValidatedField()
    {
        InitializeComponent();
        Input.SetBinding(Entry.TextProperty, new Binding(nameof(Text), source: this));
        Input.SetBinding(Entry.PlaceholderProperty, new Binding(nameof(Placeholder), source: this));
        ToggleButton.Clicked += (_, _) => BasculerMotDePasse();
        RefreshToggle(IsPasswordField);
        RefreshAide(Aide);
        RefreshErreur(Erreur);
        RefreshSucces(Succes);
    }

    private void BasculerMotDePasse()
    {
        Input.IsPassword = !Input.IsPassword;
        ToggleButton.Text = Input.IsPassword ? AppResources.Connexion_Afficher : AppResources.Connexion_Masquer;
    }

    private void RefreshToggle(bool isPasswordField)
    {
        ToggleButton.IsVisible = isPasswordField;
        if (isPasswordField)
        {
            Input.IsPassword = true;
            ToggleButton.Text = AppResources.Connexion_Afficher;
        }
    }

    private void RefreshAide(string aide)
    {
        AideLabel.Text = aide;
        AideLabel.IsVisible = aide.Length > 0;
    }

    private void RefreshErreur(string erreur)
    {
        ErreurLabel.Text = erreur;
        ErreurRow.IsVisible = erreur.Length > 0;
    }

    private void RefreshSucces(string succes)
    {
        SuccesLabel.Text = succes;
        SuccesRow.IsVisible = succes.Length > 0;
    }
}
