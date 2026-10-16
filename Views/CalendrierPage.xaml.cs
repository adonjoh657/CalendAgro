using CalendAgro.Services;
using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class CalendrierPage : ContentPage
{
    public CalendrierPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<CalendrierViewModel>()
            ?? new CalendrierViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var tabs = App.Current?.Handler?.MauiContext?.Services?.GetService<ITabNavigationService>();
        if (tabs is not null && Content is VisualElement content)
        {
            await PageSlideAnimator.SlideInAsync(content, tabs.ConsumeLastStep());
        }
        if (BindingContext is CalendrierViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
