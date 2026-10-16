using System.Collections.ObjectModel;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Catalogue : recherche directe, filtre par catégorie, états.
public sealed partial class CulturesViewModel : ObservableObject
{
    private readonly ICultureService _cultures;
    private readonly ITabNavigationService _tabs;
    private bool _started;

    public ObservableCollection<Culture> Resultats { get; } = [];

    public IReadOnlyList<string> Categories { get; } =
    [
        AppResources.Cultures_Toutes,
        AppResources.Cultures_Cereale,
        AppResources.Cultures_Tubercule,
        AppResources.Cultures_Legume,
        AppResources.Cultures_Rente,
    ];

    [ObservableProperty]
    public partial string Recherche { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CategorieChoisie { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    public bool IsListeVisible => !IsBusy && !HasError;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    public CulturesViewModel(ICultureService cultures, ITabNavigationService tabs)
    {
        _cultures = cultures;
        _tabs = tabs;
        CategorieChoisie = AppResources.Cultures_Toutes;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public CulturesViewModel() : this(new FakeCultureService(), new TabNavigationService())
    {
    }

    partial void OnRechercheChanged(string value) => _ = FiltrerAsync();

    partial void OnCategorieChoisieChanged(string value) => _ = FiltrerAsync();

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsListeVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsListeVisible));

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
            await _cultures.GetCulturesAsync();
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
    private void EffacerRecherche()
    {
        Recherche = string.Empty;
        CategorieChoisie = AppResources.Cultures_Toutes;
    }

    [RelayCommand]
    private async Task OuvrirFicheAsync(string cultureId)
    {
        try
        {
            // Route enregistrée par l'écran fiche culture (tâche suivante).
            await Shell.Current.GoToAsync($"fiche-culture?cultureId={cultureId}");
        }
        catch
        {
            NavigationErreur = AppResources.Cultures_FicheEnCours;
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
            var cultures = await _cultures.SearchAsync(Recherche, CategorieDe(CategorieChoisie));
            Resultats.Clear();
            foreach (var culture in cultures)
            {
                Resultats.Add(culture);
            }
        }
        catch
        {
            HasError = true;
        }
    }

    private static CategorieCulture? CategorieDe(string libelle)
    {
        if (libelle == AppResources.Cultures_Cereale)
        {
            return CategorieCulture.Cereale;
        }
        if (libelle == AppResources.Cultures_Tubercule)
        {
            return CategorieCulture.Tubercule;
        }
        if (libelle == AppResources.Cultures_Legume)
        {
            return CategorieCulture.Legume;
        }
        if (libelle == AppResources.Cultures_Rente)
        {
            return CategorieCulture.Rente;
        }
        return null;
    }
}
