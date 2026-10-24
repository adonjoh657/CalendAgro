using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class RappelsPage : ContentPage
{
    public RappelsPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<RappelsViewModel>()
            ?? new RappelsViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RappelsViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
