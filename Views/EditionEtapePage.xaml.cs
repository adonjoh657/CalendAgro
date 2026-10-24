using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class EditionEtapePage : ContentPage
{
    public EditionEtapePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<EditionEtapeViewModel>()
            ?? new EditionEtapeViewModel();
    }
}
