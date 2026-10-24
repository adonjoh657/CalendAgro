using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class EditionRappelPage : ContentPage
{
    public EditionRappelPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<EditionRappelViewModel>()
            ?? new EditionRappelViewModel();
    }
}
