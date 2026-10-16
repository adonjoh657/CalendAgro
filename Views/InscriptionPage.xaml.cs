using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class InscriptionPage : ContentPage
{
    public InscriptionPage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<InscriptionViewModel>()
            ?? new InscriptionViewModel();
    }
}
