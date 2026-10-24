using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Profil et paramètres : identité locale, langue, données, déconnexion.
public sealed partial class ProfilViewModel : ObservableObject
{
    private readonly IProfilService _profil;

    [ObservableProperty]
    public partial string NomAffiche { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Details { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Initiales { get; set; } = "?";

    [ObservableProperty]
    public partial bool EstConnecte { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string ErreurAction { get; set; } = string.Empty;

    public bool EstInvite => !EstConnecte;

    public bool IsContenuVisible => !IsBusy && !HasError;

    public bool ErreurActionVisible => ErreurAction.Length > 0;

    public ProfilViewModel(IProfilService profil) => _profil = profil;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public ProfilViewModel() : this(DefaultServices.Profil)
    {
    }

    partial void OnEstConnecteChanged(bool value) => OnPropertyChanged(nameof(EstInvite));

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnErreurActionChanged(string value) => OnPropertyChanged(nameof(ErreurActionVisible));

    public Task LoadAsync() => RechargerAsync();

    [RelayCommand]
    private async Task RechargerAsync()
    {
        IsBusy = true;
        HasError = false;
        ErreurAction = string.Empty;
        try
        {
            var profil = await _profil.GetProfilAsync();
            EstConnecte = profil is not null;
            if (profil is null)
            {
                NomAffiche = AppResources.Profil_Invite;
                Details = AppResources.Profil_InviteAide;
                Initiales = "?";
                return;
            }

            NomAffiche = profil.Nom;
            Details = string.IsNullOrWhiteSpace(profil.Telephone)
                ? profil.Identifiant
                : $"{profil.Identifiant} · {profil.Telephone}";
            Initiales = InitialesDe(profil.Nom);
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
    private Task CreerCompteAsync() => Shell.Current.GoToAsync("inscription");

    [RelayCommand]
    private Task SeConnecterAsync() => Shell.Current.GoToAsync("connexion");

    [RelayCommand]
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task SeDeconnecterAsync()
    {
        ErreurAction = string.Empty;
        try
        {
            await _profil.DeconnecterAsync();
            await Shell.Current.GoToAsync("///demarrage");
        }
        catch
        {
            ErreurAction = AppResources.Profil_DeconnexionErreur;
        }
    }

    private static string InitialesDe(string nom)
    {
        var mots = nom.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return mots.Length == 0
            ? "?"
            : string.Concat(mots.Take(2).Select(mot => char.ToUpperInvariant(mot[0])));
    }
}
