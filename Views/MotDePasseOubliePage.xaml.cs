using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class MotDePasseOubliePage : ContentPage
{
    public MotDePasseOubliePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<MotDePasseOublieViewModel>()
            ?? new MotDePasseOublieViewModel();
    }
}
