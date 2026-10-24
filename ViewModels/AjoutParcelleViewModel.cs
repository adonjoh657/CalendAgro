using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Ajout d'une parcelle : nom, région et culture choisis dans les listes de l'app,
// surface facultative, date de semis. Bouton désactivé tant qu'invalide.
[QueryProperty(nameof(CultureProposee), "culture")]
public sealed partial class AjoutParcelleViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IParcelleService _parcelles;
    private readonly ICultureService _cultures;
    private string _cultureProposee = string.Empty;
    private bool _loaded;

    public ObservableCollection<string> Regions { get; } = [];

    public ObservableCollection<string> Cultures { get; } = [];

    [ObservableProperty]
    public partial string Nom { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Region { get; set; }

    [ObservableProperty]
    public partial string? Culture { get; set; }

    [ObservableProperty]
    public partial string SurfaceTexte { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime DateSemis { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string NomErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RegionErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CultureErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SurfaceErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    public string CultureProposee
    {
        get => _cultureProposee;
        set
        {
            _cultureProposee = Uri.UnescapeDataString(value ?? string.Empty);
            if (_cultureProposee.Length > 0)
            {
                Culture = _cultureProposee;
            }
        }
    }

    public bool IsFormVisible => !IsBusy && !HasError;

    public bool CultureEstChoisie => !string.IsNullOrWhiteSpace(Culture);

    public bool NomErreurVisible => NomErreur.Length > 0;

    public bool RegionErreurVisible => RegionErreur.Length > 0;

    public bool CultureErreurVisible => CultureErreur.Length > 0;

    public bool SurfaceErreurVisible => SurfaceErreur.Length > 0;

    public AjoutParcelleViewModel(IParcelleService parcelles, ICultureService cultures)
    {
        _parcelles = parcelles;
        _cultures = cultures;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public AjoutParcelleViewModel() : this(DefaultServices.Parcelles, DefaultServices.Cultures)
    {
    }

    public async Task LoadAsync()
    {
        if (_loaded)
        {
            return;
        }

        IsBusy = true;
        HasError = false;
        try
        {
            var regions = await _cultures.GetZonesAsync();
            var cultures = await _cultures.GetCulturesAsync();

            Regions.Clear();
            foreach (var region in regions)
            {
                Regions.Add(region);
            }

            Cultures.Clear();
            foreach (var culture in cultures.OrderBy(c => c.Nom, StringComparer.CurrentCultureIgnoreCase))
            {
                Cultures.Add(culture.Nom);
            }

            Culture = _cultureProposee.Length > 0 && Cultures.Contains(_cultureProposee)
                ? _cultureProposee
                : null;
            _loaded = true;
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

    partial void OnNomChanged(string value)
    {
        NomErreur = string.IsNullOrWhiteSpace(value) ? AppResources.Connexion_ChampObligatoire : string.Empty;
        AjouterCommand.NotifyCanExecuteChanged();
    }

    partial void OnRegionChanged(string? value)
    {
        RegionErreur = string.IsNullOrWhiteSpace(value) ? AppResources.Connexion_ChampObligatoire : string.Empty;
        AjouterCommand.NotifyCanExecuteChanged();
    }

    partial void OnCultureChanged(string? value)
    {
        CultureErreur = string.IsNullOrWhiteSpace(value) ? AppResources.Parcelle_CultureErreur : string.Empty;
        OnPropertyChanged(nameof(CultureEstChoisie));
        AjouterCommand.NotifyCanExecuteChanged();
    }

    partial void OnSurfaceTexteChanged(string value)
    {
        SurfaceErreur = ValiderSurface(value);
        AjouterCommand.NotifyCanExecuteChanged();
    }

    partial void OnNomErreurChanged(string value) => OnPropertyChanged(nameof(NomErreurVisible));

    partial void OnRegionErreurChanged(string value) => OnPropertyChanged(nameof(RegionErreurVisible));

    partial void OnCultureErreurChanged(string value) => OnPropertyChanged(nameof(CultureErreurVisible));

    partial void OnSurfaceErreurChanged(string value) => OnPropertyChanged(nameof(SurfaceErreurVisible));

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFormVisible));
        AjouterCommand.NotifyCanExecuteChanged();
    }

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsFormVisible));

    partial void OnIsSavingChanged(bool value) => AjouterCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(PeutAjouter))]
    private async Task AjouterAsync()
    {
        IsSaving = true;
        HasError = false;
        try
        {
            await _parcelles.AjouterAsync(
                Nom,
                Culture?.Trim() ?? string.Empty,
                ConvertirSurface(SurfaceTexte),
                Region ?? string.Empty,
                DateOnly.FromDateTime(DateSemis));
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            HasError = true;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    private bool PeutAjouter() =>
        !IsBusy
        && !IsSaving
        && NomErreur.Length == 0
        && RegionErreur.Length == 0
        && CultureErreur.Length == 0
        && SurfaceErreur.Length == 0
        && !string.IsNullOrWhiteSpace(Nom)
        && !string.IsNullOrWhiteSpace(Region)
        && !string.IsNullOrWhiteSpace(Culture);

    private static string ValiderSurface(string valeur)
    {
        if (valeur.Length == 0)
        {
            return string.Empty;
        }
        return ConvertirSurface(valeur) is > 0
            ? string.Empty
            : AppResources.Parcelle_SurfaceErreur;
    }

    private static double? ConvertirSurface(string valeur)
    {
        if (valeur.Length == 0)
        {
            return null;
        }
        if (double.TryParse(valeur, NumberStyles.Number, Francais, out var hectares)
            || double.TryParse(valeur, NumberStyles.Number, CultureInfo.InvariantCulture, out hectares))
        {
            return hectares;
        }
        return double.NaN;
    }
}
