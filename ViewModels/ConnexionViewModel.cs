using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Écran de connexion : validation directe sous chaque champ,
// erreur d'identifiants dans le formulaire, erreur technique plein écran.
public sealed partial class ConnexionViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty]
    public partial string Identifiant { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MotDePasse { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool MotDePasseVisible { get; set; }

    [ObservableProperty]
    public partial string IdentifiantErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MotDePasseErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FormErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public bool MotDePasseMasque => !MotDePasseVisible;

    public string MotDePasseBouton => MotDePasseVisible
        ? AppResources.Connexion_Masquer
        : AppResources.Connexion_Afficher;

    public bool IsFormVisible => !IsBusy && !HasError;

    public bool IdentifiantErreurVisible => IdentifiantErreur.Length > 0;

    public bool MotDePasseErreurVisible => MotDePasseErreur.Length > 0;

    public bool HasFormErreur => FormErreur.Length > 0;

    public ConnexionViewModel(IAuthService auth) => _auth = auth;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public ConnexionViewModel() : this(DefaultServices.Authentification)
    {
    }

    partial void OnIdentifiantChanged(string value)
    {
        IdentifiantErreur = ValidateIdentifiant(value);
        FormErreur = string.Empty;
    }

    partial void OnMotDePasseChanged(string value)
    {
        MotDePasseErreur = string.IsNullOrWhiteSpace(value) ? AppResources.Connexion_ChampObligatoire : string.Empty;
        FormErreur = string.Empty;
    }

    partial void OnMotDePasseVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(MotDePasseMasque));
        OnPropertyChanged(nameof(MotDePasseBouton));
    }

    partial void OnIdentifiantErreurChanged(string value) => OnPropertyChanged(nameof(IdentifiantErreurVisible));

    partial void OnMotDePasseErreurChanged(string value) => OnPropertyChanged(nameof(MotDePasseErreurVisible));

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsFormVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsFormVisible));

    partial void OnFormErreurChanged(string value) => OnPropertyChanged(nameof(HasFormErreur));

    [RelayCommand]
    private void BasculerMotDePasse() => MotDePasseVisible = !MotDePasseVisible;

    [RelayCommand]
    private async Task SeConnecterAsync()
    {
        FormErreur = string.Empty;
        IdentifiantErreur = ValidateIdentifiant(Identifiant);
        MotDePasseErreur = string.IsNullOrWhiteSpace(MotDePasse) ? AppResources.Connexion_ChampObligatoire : string.Empty;
        if (IdentifiantErreur.Length > 0 || MotDePasseErreur.Length > 0)
        {
            return;
        }

        IsBusy = true;
        HasError = false;
        try
        {
            var result = await _auth.SignInAsync(Identifiant.Trim(), MotDePasse);
            if (result.Succeeded)
            {
                await Shell.Current.GoToAsync("///accueil");
            }
            else
            {
                FormErreur = AppResources.Connexion_ErreurIdentifiants;
            }
        }
        catch
        {
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Liens vers les écrans des tâches suivantes : message d'attente en attendant.
    [RelayCommand]
    private Task MotDePasseOublieAsync() => AllerVersAsync("mot-de-passe-oublie");

    [RelayCommand]
    private Task InscriptionAsync() => AllerVersAsync("inscription");

    [RelayCommand]
    private Task ContinuerSansCompteAsync() => Shell.Current.GoToAsync("///accueil");

    [RelayCommand]
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    private string ValidateIdentifiant(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return AppResources.Connexion_ChampObligatoire;
        }
        return _auth.IsIdentifiantValid(value)
            ? string.Empty
            : AppResources.Connexion_IdentifiantInvalide;
    }

    private async Task AllerVersAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch
        {
            FormErreur = AppResources.Connexion_ErreurLien;
        }
    }
}
