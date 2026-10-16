using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Ajout d'une parcelle : nom + région + culture obligatoires,
// surface facultative, date de semis. Bouton désactivé tant qu'invalide.
[QueryProperty(nameof(CultureId), "cultureId")]
public sealed partial class AjoutParcelleViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly ICultureService _cultures;
    private readonly IParcelleService _parcelles;
    private readonly Dictionary<string, string> _idsParNom = [];
    private string _cultureId = string.Empty;

    public ObservableCollection<string> CultureNoms { get; } = [];

    [ObservableProperty]
    public partial string Nom { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Region { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? CultureChoisie { get; set; }

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
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public string CultureId
    {
        get => _cultureId;
        set
        {
            _cultureId = Uri.UnescapeDataString(value ?? string.Empty);
            PreselectionnerCulture();
        }
    }

    public bool IsFormVisible => !IsBusy && !HasError;

    public bool CultureEstChoisie => CultureChoisie is not null;

    public bool NomErreurVisible => NomErreur.Length > 0;

    public bool RegionErreurVisible => RegionErreur.Length > 0;

    public bool CultureErreurVisible => CultureErreur.Length > 0;

    public bool SurfaceErreurVisible => SurfaceErreur.Length > 0;

    public AjoutParcelleViewModel(ICultureService cultures, IParcelleService parcelles)
    {
        _cultures = cultures;
        _parcelles = parcelles;
        Region = AppResources.Fiche_ZoneCentre;
        _ = ChargerCulturesAsync();
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public AjoutParcelleViewModel() : this(new FakeCultureService(), new FakeParcelleService(new FakeCultureService()))
    {
    }

    partial void OnNomChanged(string value)
    {
        NomErreur = string.IsNullOrWhiteSpace(value) ? AppResources.Connexion_ChampObligatoire : string.Empty;
        AjouterCommand.NotifyCanExecuteChanged();
    }

    partial void OnRegionChanged(string value)
    {
        RegionErreur = string.IsNullOrWhiteSpace(value) ? AppResources.Connexion_ChampObligatoire : string.Empty;
        AjouterCommand.NotifyCanExecuteChanged();
    }

    partial void OnCultureChoisieChanged(string? value)
    {
        CultureErreur = value is null ? AppResources.Parcelle_CultureErreur : string.Empty;
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

    [RelayCommand(CanExecute = nameof(PeutAjouter))]
    private async Task AjouterAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            await _parcelles.AjouterAsync(
                Nom,
                _idsParNom[CultureChoisie!],
                ConvertirSurface(SurfaceTexte),
                Region,
                DateOnly.FromDateTime(DateSemis));
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

    private bool PeutAjouter() =>
        !IsBusy
        && NomErreur.Length == 0
        && RegionErreur.Length == 0
        && CultureErreur.Length == 0
        && SurfaceErreur.Length == 0
        && Nom.Length > 0
        && Region.Length > 0
        && CultureChoisie is not null;

    private async Task ChargerCulturesAsync()
    {
        try
        {
            var cultures = await _cultures.GetCulturesAsync();
            CultureNoms.Clear();
            _idsParNom.Clear();
            foreach (var culture in cultures)
            {
                CultureNoms.Add(culture.Nom);
                _idsParNom[culture.Nom] = culture.Id;
            }
            PreselectionnerCulture();
        }
        catch
        {
            HasError = true;
        }
    }

    private void PreselectionnerCulture()
    {
        if (_cultureId.Length == 0 || _idsParNom.Count == 0)
        {
            return;
        }
        var nom = _idsParNom.FirstOrDefault(p => p.Value == _cultureId).Key;
        if (nom is not null)
        {
            CultureChoisie = nom;
        }
    }

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
