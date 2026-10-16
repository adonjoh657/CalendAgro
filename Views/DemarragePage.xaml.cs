using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class DemarragePage : ContentPage
{
    public DemarragePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<DemarrageViewModel>()
            ?? new DemarrageViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DemarrageViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
