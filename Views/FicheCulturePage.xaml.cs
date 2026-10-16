using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class FicheCulturePage : ContentPage
{
    public FicheCulturePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<FicheCultureViewModel>()
            ?? new FicheCultureViewModel();
    }
}
