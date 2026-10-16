using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Fiche d'une parcelle : infos, prochain travail, suppression.
[QueryProperty(nameof(ParcelleId), "parcelleId")]
public sealed partial class FicheParcelleViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IParcelleService _parcelles;
    private string _parcelleId = string.Empty;

    [ObservableProperty]
    public partial string Nom { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CultureNom { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Infos { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateSemis { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StepKind Kind { get; set; } = StepKind.Semis;

    [ObservableProperty]
    public partial string Phase { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Prochain { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TravailId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool AvecTravail { get; set; }

    public bool SansTravail => !AvecTravail;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool Introuvable { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    public string ParcelleId
    {
        get => _parcelleId;
        set
        {
            _parcelleId = Uri.UnescapeDataString(value ?? string.Empty);
            _ = RechargerAsync();
        }
    }

    public bool IsContenuVisible => !IsBusy && !HasError && !Introuvable;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    public FicheParcelleViewModel(IParcelleService parcelles) => _parcelles = parcelles;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public FicheParcelleViewModel()
        : this(new FakeParcelleService(new FakeCultureService()))
    {
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnIntrouvableChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnNavigationErreurChanged(string value) => OnPropertyChanged(nameof(NavigationErreurVisible));

    partial void OnAvecTravailChanged(bool value) => OnPropertyChanged(nameof(SansTravail));

    [RelayCommand]
    private async Task RechargerAsync()
    {
        IsBusy = true;
        HasError = false;
        Introuvable = false;
        try
        {
            var parcelle = await _parcelles.GetParcelleAsync(_parcelleId);
            if (parcelle is null)
            {
                Introuvable = true;
                return;
            }
            Nom = parcelle.Nom;
            CultureNom = parcelle.CultureNom;
            Infos = $"{string.Format(AppResources.Accueil_Hectares, parcelle.SurfaceHa.ToString("0.00", Francais))} · {parcelle.Region}";
            DateSemis = string.Format(
                AppResources.FicheParcelle_DateSemis,
                parcelle.DateSemis.ToString("d MMM yyyy", Francais));
            var travail = parcelle.ProchainTravail;
            AvecTravail = travail is not null;
            if (travail is not null)
            {
                Kind = BadgeDe(travail.Phase);
                Phase = TypeEtapeLabels.Libelle(travail.Phase);
                Prochain = $"{travail.Date.ToString("d MMM", Francais)} · {travail.Titre}";
                TravailId = travail.Id;
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
    private async Task OuvrirTravailAsync()
    {
        try
        {
            await Shell.Current.GoToAsync($"detail-tache?travailId={TravailId}");
        }
        catch
        {
            NavigationErreur = AppResources.Accueil_DetailEnCours;
        }
    }

    [RelayCommand]
    private async Task SupprimerAsync()
    {
        IsBusy = true;
        try
        {
            await _parcelles.SupprimerAsync(_parcelleId);
            await Shell.Current.GoToAsync("..");
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

    private static StepKind BadgeDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => StepKind.Entretien,
        TypeEtape.Recolte => StepKind.Recolte,
        _ => StepKind.Semis,
    };
}
