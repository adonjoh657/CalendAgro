using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class ChartePage : ContentPage
{
    public ChartePage()
    {
        InitializeComponent();
        // Page de test : résolution via DI avec repli local (Shell DataTemplate = ctor sans paramètre).
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<CharteViewModel>()
            ?? new CharteViewModel();
    }
}
