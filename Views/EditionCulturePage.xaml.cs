using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class EditionCulturePage : ContentPage
{
    public EditionCulturePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<EditionCultureViewModel>()
            ?? new EditionCultureViewModel();
    }
}
