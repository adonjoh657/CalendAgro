using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Écran de démarrage : chargement de la session locale, puis accès
// connexion ou continuation sans compte. Tout reste sur l'appareil.
public sealed partial class DemarrageViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private bool _started;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool HasAccount { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public bool IsReady => !IsLoading && !HasError;

    public bool ShowGuestNote => IsReady && !HasAccount;

    public DemarrageViewModel(ISessionService session) => _session = session;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public DemarrageViewModel() : this(new FakeSessionService())
    {
    }

    partial void OnIsLoadingChanged(bool value) => RefreshDerived();

    partial void OnHasErrorChanged(bool value) => RefreshDerived();

    partial void OnHasAccountChanged(bool value) => RefreshDerived();

    public async Task LoadAsync()
    {
        if (_started)
        {
            return;
        }
        _started = true;
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        IsLoading = true;
        HasError = false;
        try
        {
            var info = await _session.GetStartAsync();
            HasAccount = info.HasLocalAccount;
        }
        catch
        {
            HasError = true;
            ErrorMessage = AppResources.Demarrage_ErreurChargement;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SeConnecterAsync()
    {
        try
        {
            // Route enregistrée par l'écran de connexion (tâche suivante).
            await Shell.Current.GoToAsync("connexion");
        }
        catch
        {
            HasError = true;
            ErrorMessage = AppResources.Demarrage_ErreurConnexion;
        }
    }

    [RelayCommand]
    private Task ContinuerSansCompteAsync() => Shell.Current.GoToAsync("///accueil");

    private void RefreshDerived()
    {
        OnPropertyChanged(nameof(IsReady));
        OnPropertyChanged(nameof(ShowGuestNote));
    }
}
