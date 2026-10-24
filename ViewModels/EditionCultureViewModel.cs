using System.Globalization;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Ajout ou modification d'une culture : nom, catégorie, durée du cycle en mois.
// Sans cultureId, on crée ; avec cultureId, on modifie.
[QueryProperty(nameof(CultureId), "cultureId")]
public sealed partial class EditionCultureViewModel : ObservableObject
{
    private const int NomLongueurMinimale = 2;
    private const int NomLongueurMaximale = 40;
    private const int CycleMoisMinimum = 1;
    private const int CycleMoisMaximum = 120;
    private const int JoursParMois = 30;

    private readonly ICultureService _cultures;
    private string? _cultureId;

    public IReadOnlyList<string> Categories { get; } =
    [
        AppResources.Culture_CatCereale,
        AppResources.Culture_CatTubercule,
        AppResources.Culture_CatLegume,
        AppResources.Culture_CatRente,
    ];

    [ObservableProperty]
    public partial string Titre { get; set; } = AppResources.Culture_TitreAjout;

    [ObservableProperty]
    public partial string Nom { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CategorieIndex { get; set; }

    [ObservableProperty]
    public partial string CycleTexte { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NomErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CycleErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public string? CultureId
    {
        get => _cultureId;
        set
        {
            var identifiant = Uri.UnescapeDataString(value ?? string.Empty);
            _cultureId = identifiant.Length > 0 ? identifiant : null;
            _ = ChargerAsync();
        }
    }

    public bool IsFormVisible => !IsBusy && !HasError;

    public bool NomErreurVisible => NomErreur.Length > 0;

    public bool CycleErreurVisible => CycleErreur.Length > 0;

    public EditionCultureViewModel(ICultureService cultures) => _cultures = cultures;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public EditionCultureViewModel() : this(DefaultServices.Cultures)
    {
    }

    partial void OnNomChanged(string value)
    {
        NomErreur = ValiderNom(value);
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnCycleTexteChanged(string value)
    {
        CycleErreur = ValiderCycle(value);
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnNomErreurChanged(string value) => OnPropertyChanged(nameof(NomErreurVisible));

    partial void OnCycleErreurChanged(string value) => OnPropertyChanged(nameof(CycleErreurVisible));

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFormVisible));
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsFormVisible));

    private async Task ChargerAsync()
    {
        if (_cultureId is null)
        {
            Titre = AppResources.Culture_TitreAjout;
            return;
        }

        IsBusy = true;
        HasError = false;
        try
        {
            var culture = await _cultures.GetCultureAsync(_cultureId);
            if (culture is null)
            {
                HasError = true;
                return;
            }

            Titre = AppResources.Culture_TitreModif;
            Nom = culture.Nom;
            CategorieIndex = (int)culture.Categorie;
            var mois = Math.Max(CycleMoisMinimum, (int)Math.Round(culture.DureeCycleJours / (double)JoursParMois));
            CycleTexte = mois.ToString(CultureInfo.InvariantCulture);
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

    [RelayCommand(CanExecute = nameof(PeutEnregistrer))]
    private async Task EnregistrerAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            await _cultures.EnregistrerCultureAsync(
                _cultureId,
                Nom,
                (CategorieCulture)Math.Max(0, CategorieIndex),
                ConvertirCycle(CycleTexte) ?? CycleMoisMinimum);
            await Shell.Current.GoToAsync("..");
        }
        catch (CultureExistanteException)
        {
            NomErreur = AppResources.Culture_NomExistant;
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

    private bool PeutEnregistrer() =>
        !IsBusy
        && ValiderNom(Nom).Length == 0
        && ValiderCycle(CycleTexte).Length == 0;

    private static string ValiderNom(string valeur)
    {
        var longueur = valeur.Trim().Length;
        return longueur is >= NomLongueurMinimale and <= NomLongueurMaximale
            ? string.Empty
            : AppResources.Culture_NomErreur;
    }

    private static string ValiderCycle(string valeur) =>
        ConvertirCycle(valeur) is >= CycleMoisMinimum and <= CycleMoisMaximum
            ? string.Empty
            : AppResources.Culture_CycleErreur;

    private static int? ConvertirCycle(string valeur) =>
        int.TryParse(valeur.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var mois) ? mois : null;
}
