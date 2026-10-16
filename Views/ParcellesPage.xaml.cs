using CalendAgro.Services;
using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class ParcellesPage : ContentPage
{
    public ParcellesPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<ParcellesViewModel>()
            ?? new ParcellesViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var tabs = App.Current?.Handler?.MauiContext?.Services?.GetService<ITabNavigationService>();
        if (tabs is not null && Content is VisualElement content)
        {
            await PageSlideAnimator.SlideInAsync(content, tabs.ConsumeLastStep());
        }
        if (BindingContext is ParcellesViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
