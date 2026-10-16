using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Écran d'inscription : validation directe sous chaque champ, bouton
// désactivé tant que le formulaire est invalide, résumé après tentative.
public sealed partial class InscriptionViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    [ObservableProperty]
    public partial string Nom { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Identifiant { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MotDePasse { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Confirmation { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool MotDePasseVisible { get; set; }

    [ObservableProperty]
    public partial bool ConfirmationVisible { get; set; }

    [ObservableProperty]
    public partial string NomErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NomSucces { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IdentifiantErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MotDePasseErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MotDePasseSucces { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmationErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FormErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool ResumeVisible { get; set; }

    public bool MotDePasseMasque => !MotDePasseVisible;

    public string MotDePasseBouton => MotDePasseVisible
        ? AppResources.Connexion_Masquer
        : AppResources.Connexion_Afficher;

    public bool ConfirmationMasquee => !ConfirmationVisible;

    public string ConfirmationBouton => ConfirmationVisible
        ? AppResources.Connexion_Masquer
        : AppResources.Connexion_Afficher;

    public bool IsFormVisible => !IsBusy && !HasError;

    public bool NomSuccesVisible => NomSucces.Length > 0;

    public bool MotDePasseSuccesVisible => MotDePasseSucces.Length > 0;

    public bool NomErreurVisible => NomErreur.Length > 0;

    public bool IdentifiantErreurVisible => IdentifiantErreur.Length > 0;

    public bool MotDePasseErreurVisible => MotDePasseErreur.Length > 0;

    public bool ConfirmationErreurVisible => ConfirmationErreur.Length > 0;

    public InscriptionViewModel(IAuthService auth) => _auth = auth;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public InscriptionViewModel() : this(new FakeAuthService())
    {
    }

    partial void OnNomChanged(string value)
    {
        (NomErreur, NomSucces) = ValidateNom(value);
        ClearSubmitState();
    }

    partial void OnIdentifiantChanged(string value)
    {
        IdentifiantErreur = ValidateIdentifiant(value);
        ClearSubmitState();
    }

    partial void OnMotDePasseChanged(string value)
    {
        (MotDePasseErreur, MotDePasseSucces) = ValidateMotDePasse(value);
        ConfirmationErreur = ValidateConfirmation(Confirmation);
        ClearSubmitState();
    }

    partial void OnConfirmationChanged(string value)
    {
        ConfirmationErreur = ValidateConfirmation(value);
        ClearSubmitState();
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

    partial void OnNomSuccesChanged(string value) => OnPropertyChanged(nameof(NomSuccesVisible));

    partial void OnMotDePasseSuccesChanged(string value) => OnPropertyChanged(nameof(MotDePasseSuccesVisible));

    partial void OnNomErreurChanged(string value) => OnPropertyChanged(nameof(NomErreurVisible));

    partial void OnIdentifiantErreurChanged(string value) => OnPropertyChanged(nameof(IdentifiantErreurVisible));

    partial void OnMotDePasseErreurChanged(string value) => OnPropertyChanged(nameof(MotDePasseErreurVisible));

    partial void OnConfirmationErreurChanged(string value) => OnPropertyChanged(nameof(ConfirmationErreurVisible));

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFormVisible));
        CreerCompteCommand.NotifyCanExecuteChanged();
    }

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsFormVisible));

    [RelayCommand]
    private void BasculerMotDePasse() => MotDePasseVisible = !MotDePasseVisible;

    [RelayCommand]
    private void BasculerConfirmation() => ConfirmationVisible = !ConfirmationVisible;

    [RelayCommand(CanExecute = nameof(CanCreerCompte))]
    private async Task CreerCompteAsync()
    {
        (NomErreur, NomSucces) = ValidateNom(Nom);
        IdentifiantErreur = ValidateIdentifiant(Identifiant);
        (MotDePasseErreur, MotDePasseSucces) = ValidateMotDePasse(MotDePasse);
        ConfirmationErreur = ValidateConfirmation(Confirmation);
        if (!CanCreerCompte())
        {
            ResumeVisible = true;
            return;
        }

        IsBusy = true;
        HasError = false;
        try
        {
            var result = await _auth.SignUpAsync(Nom.Trim(), Identifiant.Trim(), MotDePasse);
            if (result.Succeeded)
            {
                await Shell.Current.GoToAsync("///accueil");
            }
            else
            {
                FormErreur = result.Erreur ?? AppResources.Connexion_Erreur;
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
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private Task AllerConnexionAsync() => Shell.Current.GoToAsync("connexion");

    private bool CanCreerCompte() =>
        !IsBusy
        && NomErreur.Length == 0
        && IdentifiantErreur.Length == 0
        && MotDePasseErreur.Length == 0
        && ConfirmationErreur.Length == 0
        && Nom.Length > 0
        && Identifiant.Length > 0
        && MotDePasse.Length > 0
        && Confirmation.Length > 0;

    private (string Erreur, string Succes) ValidateNom(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (AppResources.Connexion_ChampObligatoire, string.Empty);
        }
        return _auth.IsNomValid(value)
            ? (string.Empty, AppResources.Inscription_NomValide)
            : (AppResources.Inscription_NomCourt, string.Empty);
    }

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
        return value == MotDePasse
            ? string.Empty
            : AppResources.Inscription_ConfirmationDifferente;
    }

    private void ClearSubmitState()
    {
        FormErreur = string.Empty;
        ResumeVisible = false;
        CreerCompteCommand.NotifyCanExecuteChanged();
    }
}
