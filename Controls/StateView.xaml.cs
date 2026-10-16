using System.Windows.Input;

namespace CalendAgro.Controls;

// États vide / chargement / erreur : message + action, jamais d'écran blanc.
// Le chargement montre un indicateur, les autres états un bouton d'action.
public partial class StateView : ContentView
{
    public static readonly BindableProperty StateProperty = BindableProperty.Create(
        nameof(State), typeof(StateKind), typeof(StateView), StateKind.Empty,
        propertyChanged: (b, _, _) => ((StateView)b).Refresh());

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(StateView), string.Empty,
        propertyChanged: (b, _, n) => ((StateView)b).MessageLabel.Text = (string)n);

    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText), typeof(string), typeof(StateView), string.Empty,
        propertyChanged: (b, _, _) => ((StateView)b).Refresh());

    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand), typeof(ICommand), typeof(StateView),
        propertyChanged: (b, _, n) => ((StateView)b).ActionButton.Command = (ICommand?)n);

    public StateKind State
    {
        get => (StateKind)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public StateView()
    {
        InitializeComponent();
        MessageLabel.Text = Message;
        Refresh();
    }

    private void Refresh()
    {
        var loading = State == StateKind.Loading;
        Skeleton.IsVisible = loading;
        ActionButton.Text = ActionText;
        ActionButton.IsVisible = !loading && !string.IsNullOrWhiteSpace(ActionText);
    }
}
