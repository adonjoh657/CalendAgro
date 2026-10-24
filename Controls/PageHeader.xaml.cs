using System.Windows.Input;

namespace CalendAgro.Controls;

// En-tête commun : retour 48 x 48, titre et zone d'actions optionnelle.
public partial class PageHeader : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(PageHeader), string.Empty,
        propertyChanged: (b, _, n) => ((PageHeader)b).TitreLabel.Text = (string?)n ?? string.Empty);

    public static readonly BindableProperty ShowBackProperty = BindableProperty.Create(
        nameof(ShowBack), typeof(bool), typeof(PageHeader), true,
        propertyChanged: (b, _, n) => ((PageHeader)b).RetourButton.IsVisible = (bool)n);

    public static readonly BindableProperty BackCommandProperty = BindableProperty.Create(
        nameof(BackCommand), typeof(ICommand), typeof(PageHeader));

    public static readonly BindableProperty ActionsProperty = BindableProperty.Create(
        nameof(Actions), typeof(View), typeof(PageHeader),
        propertyChanged: (b, _, n) => ((PageHeader)b).RefreshActions((View?)n));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }

    public ICommand? BackCommand
    {
        get => (ICommand?)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public View? Actions
    {
        get => (View?)GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public PageHeader()
    {
        InitializeComponent();
    }

    private void RefreshActions(View? view)
    {
        ActionsHost.Content = view;
        ActionsHost.IsVisible = view is not null;
    }

    // Sans commande explicite, le retour reste possible sur toute page empilée.
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (BackCommand is { } command)
        {
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }
            return;
        }

        if (Shell.Current is { } shell)
        {
            await shell.GoToAsync("..");
        }
    }
}
