using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class ComposantsPage : ContentPage
{
    public ComposantsPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<ComposantsViewModel>()
            ?? new ComposantsViewModel();
    }
}
