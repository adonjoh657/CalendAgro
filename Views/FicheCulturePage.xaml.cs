using CalendAgro.ViewModels;

namespace CalendAgro.Views;

public partial class FicheCulturePage : ContentPage
{
    private bool _dejaAffichee;

    public FicheCulturePage()
    {
        InitializeComponent();
        BindingContext = App.Current?.Handler?.MauiContext?.Services?.GetService<FicheCultureViewModel>()
            ?? new FicheCultureViewModel();
    }

    // Au retour d'une modification, la fiche se recharge ; le premier affichage est déjà chargé par la navigation.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_dejaAffichee && BindingContext is FicheCultureViewModel viewModel)
        {
            viewModel.RechargerCommand.Execute(null);
        }

        _dejaAffichee = true;
    }
}
