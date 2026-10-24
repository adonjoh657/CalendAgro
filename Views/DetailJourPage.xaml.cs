using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class DetailJourPage : ContentPage
{
    public DetailJourPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<DetailJourViewModel>()
            ?? new DetailJourViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DetailJourViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
