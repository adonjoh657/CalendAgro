using System.Collections.ObjectModel;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Catalogue : recherche directe et états.
public sealed partial class CulturesViewModel : ObservableObject
{
    private readonly ICultureService _cultures;
    private readonly ITabNavigationService _tabs;
    private bool _started;

    public ObservableCollection<Culture> Resultats { get; } = [];

    [ObservableProperty]
    public partial string Recherche { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NbResultats { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool RechercheVisible { get; set; }

    public bool IsListeVisible => !IsBusy && !HasError;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    // Catalogue de référence vide (aucune recherche)
    // ou aucun résultat pour la recherche en cours.
    public bool CatalogueVide => Resultats.Count == 0 && Recherche.Length == 0;

    public string MessageVide =>
        CatalogueVide ? AppResources.Cultures_Aucune : AppResources.Cultures_AucunResultat;

    public string ActionVide => CatalogueVide ? string.Empty : AppResources.Cultures_Effacer;

    public bool NbResultatsVisible => Resultats.Count > 0;

    public CulturesViewModel(ICultureService cultures, ITabNavigationService tabs)
    {
        _cultures = cultures;
        _tabs = tabs;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public CulturesViewModel() : this(DefaultServices.Cultures, DefaultServices.Navigation)
    {
    }

    partial void OnRechercheChanged(string value) => _ = FiltrerAsync();

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsListeVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsListeVisible));

    partial void OnNavigationErreurChanged(string value) => OnPropertyChanged(nameof(NavigationErreurVisible));

    public async Task LoadAsync()
    {
        if (_started)
        {
            // Retour sur l'écran (après ajout, modification ou suppression) : liste à jour sans écran de chargement.
            await FiltrerAsync();
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
            await FiltrerAsync();
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
    private void BasculerRecherche()
    {
        RechercheVisible = !RechercheVisible;
        if (!RechercheVisible)
        {
            Recherche = string.Empty;
        }
    }

    [RelayCommand]
    private void EffacerRecherche()
    {
        Recherche = string.Empty;
    }

    [RelayCommand]
    private async Task OuvrirFicheAsync(string cultureId)
    {
        try
        {
            await Shell.Current.GoToAsync($"fiche-culture?cultureId={cultureId}");
        }
        catch
        {
            NavigationErreur = AppResources.Cultures_FicheEnCours;
        }
    }

    [RelayCommand]
    private async Task AjouterCultureAsync()
    {
        try
        {
            // La culture créée n'a pas encore d'étapes : on quitte la recherche en cours
            // pour qu'elle soit visible au retour, sans filtre qui la masque.
            Recherche = string.Empty;
            RechercheVisible = false;
            await Shell.Current.GoToAsync("edition-culture");
        }
        catch
        {
            NavigationErreur = AppResources.Connexion_ErreurLien;
        }
    }

    [RelayCommand]
    private Task GoNext() => _tabs.GoNextAsync();

    [RelayCommand]
    private Task GoPrevious() => _tabs.GoPreviousAsync();

    private async Task FiltrerAsync()
    {
        try
        {
            NavigationErreur = string.Empty;
            var cultures = await _cultures.SearchAsync(Recherche);
            Resultats.Clear();
            foreach (var culture in cultures)
            {
                Resultats.Add(culture);
            }
            NbResultats = string.Format(AppResources.Cultures_NbDisponibles, Resultats.Count);
            NotifierEtatsVides();
        }
        catch
        {
            HasError = true;
        }
    }

    private void NotifierEtatsVides()
    {
        OnPropertyChanged(nameof(CatalogueVide));
        OnPropertyChanged(nameof(MessageVide));
        OnPropertyChanged(nameof(ActionVide));
        OnPropertyChanged(nameof(NbResultatsVisible));
    }
}
