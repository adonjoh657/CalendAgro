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
    public AccueilViewModel() : this(DefaultServices.Accueil, DefaultServices.Navigation)
    {
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnNavigationErreurChanged(string value) => OnPropertyChanged(nameof(NavigationErreurVisible));

    public async Task LoadAsync()
    {
        var premier = !_started;
        _started = true;
        await RechargerAsync(premier);
    }

    [RelayCommand]
    private Task Recharger() => RechargerAsync(avecSquelette: true);

    // Le premier chargement affiche le squelette ; les retours d'écran rafraîchissent
    // sans clignotement, pour voir tout de suite une parcelle ajoutée.
    private async Task RechargerAsync(bool avecSquelette)
    {
        if (avecSquelette)
        {
            IsBusy = true;
        }
        HasError = false;
        try
        {
            var donnees = await _accueil.GetAccueilAsync();
            var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
            NbTravaux = donnees.NbTravaux;
            NbParcelles = donnees.NbParcelles;
            CulturesResume = donnees.CulturesResume.Length > 0
                ? donnees.CulturesResume
                : AppResources.Accueil_AucuneParcelle;
            SurfaceTotale = donnees.NbParcelles > 0
                ? string.Format(
                    AppResources.Accueil_AuTotal,
                    string.Format(AppResources.Accueil_Hectares, donnees.SurfaceTotaleHa.ToString("0.00", Francais)))
                : string.Empty;
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

    [RelayCommand]
    private Task OuvrirRappels() => Shell.Current.GoToAsync("rappels");

    [RelayCommand]
    private Task OuvrirProfil() => Shell.Current.GoToAsync("profil");

    [RelayCommand]
    private Task OuvrirCalendrier() => Shell.Current.GoToAsync("//calendrier");

    [RelayCommand]
    private Task OuvrirParcelles() => Shell.Current.GoToAsync("//parcelles");

    private static StepKind BadgeDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => StepKind.Entretien,
        TypeEtape.Recolte => StepKind.Recolte,
        _ => StepKind.Semis,
    };

    private static string Minuscule(string valeur) =>
        valeur.Length == 0
            ? valeur
            : char.ToLowerInvariant(valeur[0]) + valeur[1..];
}
