using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class AjoutParcellePage : ContentPage
{
    public AjoutParcellePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<AjoutParcelleViewModel>()
            ?? new AjoutParcelleViewModel();
    }
}
