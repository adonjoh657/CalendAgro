using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class ConnexionPage : ContentPage
{
    public ConnexionPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<ConnexionViewModel>()
            ?? new ConnexionViewModel();
    }
}
