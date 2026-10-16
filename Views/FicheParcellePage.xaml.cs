using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class FicheParcellePage : ContentPage
{
    public FicheParcellePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<FicheParcelleViewModel>()
            ?? new FicheParcelleViewModel();
    }
}
