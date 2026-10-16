using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Carte parcelle : culture, surface, nom, prochain travail.
public sealed class ParcelleCarte
{
    public required string Id { get; init; }

    public required string CultureNom { get; init; }

    public required string Surface { get; init; }

    public required string Nom { get; init; }

    public StepKind Kind { get; init; }

    public required string Phase { get; init; }

    public required string Prochain { get; init; }

    public required string TravailId { get; init; }

    public bool SansTravail { get; init; }

    public bool AvecTravail => !SansTravail;
}

// Mes parcelles : rechargées à chaque visite pour voir les ajouts.
public sealed partial class ParcellesViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IParcelleService _parcelles;
    private readonly ITabNavigationService _tabs;

    public ObservableCollection<ParcelleCarte> Items { get; } = [];

    [ObservableProperty]
    public partial string Resume { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    public bool IsContenuVisible => !IsBusy && !HasError;

    public bool ListeVide => IsContenuVisible && Items.Count == 0;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    public ParcellesViewModel(IParcelleService parcelles, ITabNavigationService tabs)
    {
        _parcelles = parcelles;
        _tabs = tabs;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public ParcellesViewModel() : this(new FakeParcelleService(new FakeCultureService()), new TabNavigationService())
    {
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsContenuVisible));
        OnPropertyChanged(nameof(ListeVide));
    }

    partial void OnHasErrorChanged(bool value)
    {
        OnPropertyChanged(nameof(IsContenuVisible));
        OnPropertyChanged(nameof(ListeVide));
    }

    partial void OnNavigationErreurChanged(string value) => OnPropertyChanged(nameof(NavigationErreurVisible));

    public Task LoadAsync() => RechargerAsync();

    [RelayCommand]
    private async Task RechargerAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            var parcelles = await _parcelles.GetParcellesAsync();
            var total = parcelles.Sum(p => p.SurfaceHa);
            Resume = string.Format(
                AppResources.Parcelles_Resume,
                parcelles.Count,
                string.Format(AppResources.Accueil_Hectares, total.ToString("0.00", Francais)));
            Items.Clear();
            foreach (var parcelle in parcelles)
            {
                var travail = parcelle.ProchainTravail;
                Items.Add(new ParcelleCarte
                {
                    Id = parcelle.Id,
                    CultureNom = parcelle.CultureNom,
                    Surface = string.Format(AppResources.Accueil_Hectares, parcelle.SurfaceHa.ToString("0.00", Francais)),
                    Nom = parcelle.Nom,
                    Kind = BadgeDe(travail?.Phase ?? TypeEtape.Semis),
                    Phase = travail is null ? string.Empty : TypeEtapeLabels.Libelle(travail.Phase),
                    Prochain = travail is null
                        ? string.Empty
                        : $"{travail.Date.ToString("d MMM", Francais)} · {travail.Titre}",
                    TravailId = travail?.Id ?? string.Empty,
                    SansTravail = travail is null,
                });
            }
            OnPropertyChanged(nameof(ListeVide));
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
    private async Task OuvrirFicheParcelleAsync(string parcelleId)
    {
        try
        {
            // Route enregistrée par l'écran fiche parcelle (tâche suivante).
            await Shell.Current.GoToAsync($"fiche-parcelle?parcelleId={parcelleId}");
        }
        catch
        {
            NavigationErreur = AppResources.Parcelles_FicheEnCours;
        }
    }

    [RelayCommand]
    private async Task OuvrirTravailAsync(string travailId)
    {
        try
        {
            await Shell.Current.GoToAsync($"detail-tache?travailId={travailId}");
        }
        catch
        {
            NavigationErreur = AppResources.Accueil_DetailEnCours;
        }
    }

    [RelayCommand]
    private Task AjouterParcelleAsync() => Shell.Current.GoToAsync("ajout-parcelle");

    [RelayCommand]
    private Task GoNext() => _tabs.GoNextAsync();

    [RelayCommand]
    private Task GoPrevious() => _tabs.GoPreviousAsync();

    private static StepKind BadgeDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => StepKind.Entretien,
        TypeEtape.Recolte => StepKind.Recolte,
        _ => StepKind.Semis,
    };
}
