using CalendAgro.Services;
using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class CulturesPage : ContentPage
{
    public CulturesPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<CulturesViewModel>()
            ?? new CulturesViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var tabs = App.Current?.Handler?.MauiContext?.Services?.GetService<ITabNavigationService>();
        if (tabs is not null && Content is VisualElement content)
        {
            await PageSlideAnimator.SlideInAsync(content, tabs.ConsumeLastStep());
        }
        if (BindingContext is CulturesViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
