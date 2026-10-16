using CalendAgro.Services;
using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class AccueilPage : ContentPage
{
    public AccueilPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<AccueilViewModel>()
            ?? new AccueilViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var tabs = App.Current?.Handler?.MauiContext?.Services?.GetService<ITabNavigationService>();
        if (tabs is not null && Content is VisualElement content)
        {
            await PageSlideAnimator.SlideInAsync(content, tabs.ConsumeLastStep());
        }
        if (BindingContext is AccueilViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
