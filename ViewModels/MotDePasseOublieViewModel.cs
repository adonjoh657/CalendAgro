using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Récupération en 2 étapes : identifiant puis code + nouveau mot de passe.
// Boutons désactivés tant que l'étape est invalide.
public sealed partial class MotDePasseOublieViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty]
    public partial int Etape { get; set; } = 1;

    [ObservableProperty]
    public partial string Identifiant { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IdentifiantErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Code { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CodeErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NouveauMotDePasse { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NouveauMotDePasseErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NouveauMotDePasseSucces { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Confirmation { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmationErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool MotDePasseVisible { get; set; }

    [ObservableProperty]
    public partial bool ConfirmationVisible { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public bool MotDePasseMasque => !MotDePasseVisible;

    public string MotDePasseBouton => MotDePasseVisible
        ? AppResources.Connexion_Masquer
        : AppResources.Connexion_Afficher;

    public bool ConfirmationMasquee => !ConfirmationVisible;

    public string ConfirmationBouton => ConfirmationVisible
        ? AppResources.Connexion_Masquer
        : AppResources.Connexion_Afficher;

    public bool IsFormVisible => !IsBusy && !HasError;

    public bool Etape1Visible => Etape == 1 && IsFormVisible;

    public bool Etape2Visible => Etape == 2 && IsFormVisible;

    public bool IdentifiantErreurVisible => IdentifiantErreur.Length > 0;

    public bool CodeErreurVisible => CodeErreur.Length > 0;

    public bool NouveauMotDePasseErreurVisible => NouveauMotDePasseErreur.Length > 0;

    public bool NouveauMotDePasseSuccesVisible => NouveauMotDePasseSucces.Length > 0;

    public bool ConfirmationErreurVisible => ConfirmationErreur.Length > 0;

    public string ChargementMessage => Etape == 1
        ? AppResources.Mdp_Chargement
        : AppResources.Mdp_Verification;

    public MotDePasseOublieViewModel(IAuthService auth) => _auth = auth;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public MotDePasseOublieViewModel() : this(DefaultServices.Authentification)
    {
    }

    partial void OnEtapeChanged(int value)
    {
        RefreshEtapes();
        OnPropertyChanged(nameof(ChargementMessage));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFormVisible));
        RefreshEtapes();
        EnvoyerCodeCommand.NotifyCanExecuteChanged();
        ValiderCommand.NotifyCanExecuteChanged();
    }

    partial void OnHasErrorChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFormVisible));
        RefreshEtapes();
    }

    partial void OnIdentifiantChanged(string value)
    {
        IdentifiantErreur = ValidateIdentifiant(value);
        EnvoyerCodeCommand.NotifyCanExecuteChanged();
    }

    partial void OnCodeChanged(string value)
    {
        CodeErreur = ValidateCode(value);
        ValiderCommand.NotifyCanExecuteChanged();
    }

    partial void OnNouveauMotDePasseChanged(string value)
    {
        (NouveauMotDePasseErreur, NouveauMotDePasseSucces) = ValidateMotDePasse(value);
        ConfirmationErreur = ValidateConfirmation(Confirmation);
        ValiderCommand.NotifyCanExecuteChanged();
    }

    partial void OnConfirmationChanged(string value)
    {
        ConfirmationErreur = ValidateConfirmation(value);
        ValiderCommand.NotifyCanExecuteChanged();
    }

    partial void OnMotDePasseVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(MotDePasseMasque));
        OnPropertyChanged(nameof(MotDePasseBouton));
    }

    partial void OnConfirmationVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(ConfirmationMasquee));
        OnPropertyChanged(nameof(ConfirmationBouton));
    }

    partial void OnIdentifiantErreurChanged(string value) => OnPropertyChanged(nameof(IdentifiantErreurVisible));

    partial void OnCodeErreurChanged(string value) => OnPropertyChanged(nameof(CodeErreurVisible));

    partial void OnNouveauMotDePasseErreurChanged(string value) => OnPropertyChanged(nameof(NouveauMotDePasseErreurVisible));

    partial void OnNouveauMotDePasseSuccesChanged(string value) => OnPropertyChanged(nameof(NouveauMotDePasseSuccesVisible));

    partial void OnConfirmationErreurChanged(string value) => OnPropertyChanged(nameof(ConfirmationErreurVisible));

    [RelayCommand]
    private void BasculerMotDePasse() => MotDePasseVisible = !MotDePasseVisible;

    [RelayCommand]
    private void BasculerConfirmation() => ConfirmationVisible = !ConfirmationVisible;

    [RelayCommand(CanExecute = nameof(CanEnvoyerCode))]
    private async Task EnvoyerCodeAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            await _auth.DemanderCodeAsync(Identifiant.Trim());
            Etape = 2;
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

    [RelayCommand(CanExecute = nameof(CanValider))]
    private async Task ValiderAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            var result = await _auth.RedefinirMotDePasseAsync(Code.Trim(), NouveauMotDePasse);
            if (result.Succeeded)
            {
                await Shell.Current.GoToAsync("connexion");
            }
            else
            {
                CodeErreur = result.Erreur ?? AppResources.Connexion_Erreur;
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

    [RelayCommand]
    private async Task ReessayerAsync()
    {
        if (Etape == 1)
        {
            await EnvoyerCodeAsync();
        }
        else
        {
            await ValiderAsync();
        }
    }

    [RelayCommand]
    private Task RetourConnexionAsync() => Shell.Current.GoToAsync("connexion");

    [RelayCommand]
    private Task ContinuerSansCompteAsync() => Shell.Current.GoToAsync("///accueil");

    [RelayCommand]
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    private bool CanEnvoyerCode() =>
        !IsBusy && Identifiant.Length > 0 && IdentifiantErreur.Length == 0;

    private bool CanValider() =>
        !IsBusy
        && CodeErreur.Length == 0
        && NouveauMotDePasseErreur.Length == 0
        && ConfirmationErreur.Length == 0
        && Code.Length > 0
        && NouveauMotDePasse.Length > 0
        && Confirmation.Length > 0;

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

    private string ValidateCode(string value)
    {
        if (value.Length == 0)
        {
            return AppResources.Connexion_ChampObligatoire;
        }
        return value.Trim().Length == CodeLongueur && value.Trim().All(char.IsDigit)
            ? string.Empty
            : AppResources.Mdp_CodeInvalide;
    }

    private (string Erreur, string Succes) ValidateMotDePasse(string value)
    {
        if (value.Length == 0)
        {
            return (AppResources.Connexion_ChampObligatoire, string.Empty);
        }
        return _auth.IsMotDePasseValid(value)
            ? (string.Empty, AppResources.Inscription_MotDePasseValide)
            : (AppResources.Inscription_MotDePasseCourt, string.Empty);
    }

    private string ValidateConfirmation(string value)
    {
        if (value.Length == 0)
        {
            return string.Empty;
        }
        return value == NouveauMotDePasse
            ? string.Empty
            : AppResources.Inscription_ConfirmationDifferente;
    }

    private void RefreshEtapes()
    {
        OnPropertyChanged(nameof(Etape1Visible));
        OnPropertyChanged(nameof(Etape2Visible));
    }

    private const int CodeLongueur = 6;
}
