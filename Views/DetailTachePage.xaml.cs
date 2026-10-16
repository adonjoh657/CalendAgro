using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class DetailTachePage : ContentPage
{
    public DetailTachePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<DetailTacheViewModel>()
            ?? new DetailTacheViewModel();
    }
}
