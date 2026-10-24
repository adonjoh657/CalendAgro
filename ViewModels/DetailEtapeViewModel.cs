using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Détail d'une étape : période + conseils (12 mots maximum).
[QueryProperty(nameof(CultureId), "cultureId")]
[QueryProperty(nameof(PhaseNom), "phase")]
[QueryProperty(nameof(Zone), "zone")]
[QueryProperty(nameof(Saison), "saison")]
public sealed partial class DetailEtapeViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly ICultureService _cultures;

    public ObservableCollection<string> Conseils { get; } = [];

    [ObservableProperty]
    public partial string NomCulture { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhaseTitre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StepKind Kind { get; set; } = StepKind.Semis;

    [ObservableProperty]
    public partial string Periode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ZoneSaison { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool Introuvable { get; set; }

    private string _cultureId = string.Empty;
    private string _phaseNom = string.Empty;
    private string _zone = string.Empty;
    private string _saison = string.Empty;

    public string CultureId
    {
        get => _cultureId;
        set
        {
            _cultureId = Uri.UnescapeDataString(value ?? string.Empty);
            _ = RechargerAsync();
        }
    }

    public string PhaseNom
    {
        get => _phaseNom;
        set
        {
            _phaseNom = Uri.UnescapeDataString(value ?? string.Empty);
            _ = RechargerAsync();
        }
    }

    public string Zone
    {
        get => _zone;
        set
        {
            _zone = Uri.UnescapeDataString(value ?? string.Empty);
            _ = RechargerAsync();
        }
    }

    public string Saison
    {
        get => _saison;
        set
        {
            _saison = Uri.UnescapeDataString(value ?? string.Empty);
            _ = RechargerAsync();
        }
    }

    public bool IsContenuVisible => !IsBusy && !HasError && !Introuvable;

    public DetailEtapeViewModel(ICultureService cultures) => _cultures = cultures;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public DetailEtapeViewModel() : this(DefaultServices.Cultures)
    {
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnIntrouvableChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    [RelayCommand]
    private async Task RechargerAsync()
    {
        IsBusy = true;
        HasError = false;
        Introuvable = false;
        try
        {
            if (!Enum.TryParse<TypeEtape>(PhaseNom, out var phase))
            {
                Introuvable = true;
                return;
            }
            var culture = await _cultures.GetCultureAsync(CultureId);
            var etapes = await _cultures.GetEtapesAsync(CultureId, Zone, Saison);
            var etape = etapes.FirstOrDefault(e => e.Type == phase);
            if (culture is null || etape is null)
            {
                Introuvable = true;
                return;
            }
            NomCulture = culture.Nom;
            PhaseTitre = TypeEtapeLabels.Libelle(phase);
            Kind = phase switch
            {
                TypeEtape.Entretien => StepKind.Entretien,
                TypeEtape.Recolte => StepKind.Recolte,
                _ => StepKind.Semis,
            };
            Periode = etape.MoisDebut == etape.MoisFin
                ? string.Format(AppResources.Etape_EnMois, NomMois(etape.MoisDebut))
                : string.Format(AppResources.Etape_PeriodeMois, NomMois(etape.MoisDebut), NomMois(etape.MoisFin));
            ZoneSaison = $"{etape.Zone} · {etape.Saison}";
            Conseils.Clear();
            foreach (var conseil in (await _cultures.GetConseilsAsync(CultureId))
                .Where(c => c.Phase == phase)
                .OrderBy(c => c.Ordre))
            {
                Conseils.Add(conseil.Texte);
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
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    private static string NomMois(int mois)
    {
        var nom = CultureInfo.GetCultureInfo("fr-FR").DateTimeFormat.MonthNames[mois - 1];
        return nom.Length == 0 ? nom : char.ToUpperInvariant(nom[0]) + nom[1..];
    }
}
