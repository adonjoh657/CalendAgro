using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class DetailEtapePage : ContentPage
{
    public DetailEtapePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<DetailEtapeViewModel>()
            ?? new DetailEtapeViewModel();
    }
}
