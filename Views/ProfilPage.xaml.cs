using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class ProfilPage : ContentPage
{
    public ProfilPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<ProfilViewModel>()
            ?? new ProfilViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfilViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
