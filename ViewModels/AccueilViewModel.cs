using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Carte de travail : badge de phase + libellés prêts à afficher.
public sealed class TravailCarte
{
    public required string Id { get; init; }

    public required string Titre { get; init; }

    public required string Parcelle { get; init; }

    public required string Jour { get; init; }

    public required string Phase { get; init; }

    public StepKind Kind { get; init; }
}

// Accueil : salutation, chiffres, travaux, résumé parcelles.
public sealed partial class AccueilViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IAccueilService _accueil;
    private readonly ITabNavigationService _tabs;
    private bool _started;

    public ObservableCollection<TravailCarte> Travaux { get; } = [];

    [ObservableProperty]
    public partial string Bonjour { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DateLigne { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int NbTravaux { get; set; }

    [ObservableProperty]
    public partial int NbParcelles { get; set; }

    [ObservableProperty]
    public partial string CulturesResume { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SurfaceTotale { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string VenirAussi { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    public bool IsContenuVisible => !IsBusy && !HasError;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    public AccueilViewModel(IAccueilService accueil, ITabNavigationService tabs)
    {
        _accueil = accueil;
        _tabs = tabs;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public AccueilViewModel() : this(new FakeAccueilService(), new TabNavigationService())
    {
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnNavigationErreurChanged(string value) => OnPropertyChanged(nameof(NavigationErreurVisible));

    public async Task LoadAsync()
    {
        if (_started)
        {
            return;
        }
        _started = true;
        await RechargerAsync();
    }

    [RelayCommand]
    private async Task RechargerAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            var donnees = await _accueil.GetAccueilAsync();
            var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
            Bonjour = string.Format(AppResources.Accueil_Bonjour, donnees.UserName);
            DateLigne = Capitaliser(aujourdHui.ToString("dddd d MMMM yyyy", Francais)) + " · " + AppResources.Accueil_Pays;
            NbTravaux = donnees.NbTravaux;
            NbParcelles = donnees.NbParcelles;
            CulturesResume = donnees.CulturesResume;
            SurfaceTotale = string.Format(
                AppResources.Accueil_AuTotal,
                string.Format(AppResources.Accueil_Hectares, donnees.SurfaceTotaleHa.ToString("0.00", Francais)));
            Travaux.Clear();
            foreach (var travail in donnees.Travaux)
            {
                Travaux.Add(new TravailCarte
                {
                    Id = travail.Id,
                    Titre = travail.Titre,
                    Parcelle = $"{travail.ParcelleNom} · {travail.CultureNom}",
                    Jour = travail.Date == aujourdHui
                        ? AppResources.Accueil_Aujourdhui
                        : travail.Date.ToString("d MMM", Francais),
                    Phase = TypeEtapeLabels.Libelle(travail.Phase),
                    Kind = BadgeDe(travail.Phase),
                });
            }
            VenirAussi = donnees.Travaux.Count >= 3
                ? string.Format(
                    AppResources.Accueil_VenirAussi,
                    Minuscule(donnees.Travaux[2].Titre),
                    donnees.Travaux[2].Date.ToString("d MMMM", Francais))
                : string.Empty;
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
    private async Task OuvrirDetailAsync(string travailId)
    {
        try
        {
            // Route enregistrée par l'écran détail d'une tâche (tâche suivante).
            await Shell.Current.GoToAsync($"detail-tache?travailId={travailId}");
        }
        catch
        {
            NavigationErreur = AppResources.Accueil_DetailEnCours;
        }
    }

    [RelayCommand]
    private Task GoNext() => _tabs.GoNextAsync();

    [RelayCommand]
    private Task GoPrevious() => _tabs.GoPreviousAsync();

    // Entrée provisoire vers la page de démo (retirée quand l'accueil aura son contenu).
    [RelayCommand]
    private Task OpenDemo() => Shell.Current.GoToAsync("composants");

    private static StepKind BadgeDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => StepKind.Entretien,
        TypeEtape.Recolte => StepKind.Recolte,
        _ => StepKind.Semis,
    };

    private static string Capitaliser(string valeur) =>
        valeur.Length == 0
            ? valeur
            : char.ToUpperInvariant(valeur[0]) + valeur[1..];

    private static string Minuscule(string valeur) =>
        valeur.Length == 0
            ? valeur
            : char.ToLowerInvariant(valeur[0]) + valeur[1..];
}
