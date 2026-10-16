using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Ligne de la frise : mois et phase éventuelle (« Hors cycle » sinon).
public sealed class MoisLigne
{
    public required string Nom { get; init; }

    public StepKind Kind { get; init; }

    public string PhaseLabel { get; init; } = string.Empty;

    public bool AvecPhase { get; init; }

    public bool SansPhase => !AvecPhase;
}

// Conseils d'une phase : badge + textes.
public sealed class PhaseConseils
{
    public StepKind Kind { get; init; }

    public required string Titre { get; init; }

    public required IReadOnlyList<string> Textes { get; init; }
}

// Fiche culture : frise 12 mois + conseils, exemple Centre non officiel.
[QueryProperty(nameof(CultureId), "cultureId")]
public sealed partial class FicheCultureViewModel : ObservableObject
{
    private readonly ICultureService _cultures;
    private string _cultureId = string.Empty;

    public ObservableCollection<MoisLigne> Mois { get; } = [];

    public ObservableCollection<PhaseConseils> Phases { get; } = [];

    public ObservableCollection<string> Zones { get; } = [];

    public ObservableCollection<string> Saisons { get; } = [];

    [ObservableProperty]
    public partial string? ZoneChoisie { get; set; }

    [ObservableProperty]
    public partial string? SaisonChoisie { get; set; }

    [ObservableProperty]
    public partial string Nom { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Cycle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Region { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool Introuvable { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    public string CultureId
    {
        get => _cultureId;
        set
        {
            _cultureId = Uri.UnescapeDataString(value ?? string.Empty);
            _ = RechargerAsync();
        }
    }

    public bool IsContenuVisible => !IsBusy && !HasError && !Introuvable;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    public FicheCultureViewModel(ICultureService cultures) => _cultures = cultures;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public FicheCultureViewModel() : this(new FakeCultureService())
    {
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnIntrouvableChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnNavigationErreurChanged(string value) => OnPropertyChanged(nameof(NavigationErreurVisible));

    partial void OnZoneChoisieChanged(string? value)
    {
        if (value is not null && !IsBusy)
        {
            _ = RechargerAsync();
        }
    }

    partial void OnSaisonChoisieChanged(string? value)
    {
        if (value is not null && !IsBusy)
        {
            _ = RechargerAsync();
        }
    }

    [RelayCommand]
    private async Task RechargerAsync()
    {
        IsBusy = true;
        HasError = false;
        Introuvable = false;
        try
        {
            var culture = await _cultures.GetCultureAsync(_cultureId);
            if (culture is null)
            {
                Introuvable = true;
                return;
            }
            Nom = culture.Nom;
            Cycle = string.Format(AppResources.Fiche_CycleIndicatif, culture.CycleTexte);
            await ChargerListesAsync();
            ZoneChoisie ??= AppResources.Fiche_ZoneCentre;
            SaisonChoisie ??= AppResources.Fiche_SaisonPetitePluies;
            Region = string.Format(AppResources.Fiche_RegionExemple, ZoneChoisie);

            var etapes = await _cultures.GetEtapesAsync(_cultureId, ZoneChoisie, SaisonChoisie);
            RemplirMois(etapes);

            var conseils = await _cultures.GetConseilsAsync(_cultureId);
            RemplirPhases(conseils);
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

    [RelayCommand]
    private Task RetourCatalogueAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task AjouterParcelleAsync()
    {
        try
        {
            // Route enregistrée par l'écran ajout de parcelle (tâche suivante).
            await Shell.Current.GoToAsync($"ajout-parcelle?cultureId={_cultureId}");
        }
        catch
        {
            NavigationErreur = AppResources.Fiche_ParcelleEnCours;
        }
    }

    private async Task ChargerListesAsync()
    {
        if (Zones.Count == 0)
        {
            foreach (var zone in await _cultures.GetZonesAsync())
            {
                Zones.Add(zone);
            }
        }
        if (Saisons.Count == 0)
        {
            foreach (var saison in await _cultures.GetSaisonsAsync())
            {
                Saisons.Add(saison);
            }
        }
    }

    private static bool DansPeriode(int mois, Etape etape) =>
        etape.MoisDebut <= etape.MoisFin
            ? mois >= etape.MoisDebut && mois <= etape.MoisFin
            : mois >= etape.MoisDebut || mois <= etape.MoisFin;

    private void RemplirMois(IReadOnlyList<Etape> etapes)
    {
        Mois.Clear();
        var noms = CultureInfo.GetCultureInfo("fr-FR").DateTimeFormat.MonthNames;
        for (var m = 1; m <= 12; m++)
        {
            var etape = etapes.FirstOrDefault(e => DansPeriode(m, e));
            var kind = etape?.Type switch
            {
                TypeEtape.Entretien => StepKind.Entretien,
                TypeEtape.Recolte => StepKind.Recolte,
                _ => StepKind.Semis,
            };
            Mois.Add(new MoisLigne
            {
                Nom = Capitaliser(noms[m - 1]),
                Kind = kind,
                PhaseLabel = TypeEtapeLabels.Libelle(PhaseDe(kind)),
                AvecPhase = etape is not null,
            });
        }
    }

    private void RemplirPhases(IReadOnlyList<Conseil> conseils)
    {
        Phases.Clear();
        foreach (var phase in new[] { TypeEtape.Semis, TypeEtape.Entretien, TypeEtape.Recolte })
        {
            var kind = phase switch
            {
                TypeEtape.Semis => StepKind.Semis,
                TypeEtape.Entretien => StepKind.Entretien,
                _ => StepKind.Recolte,
            };
            var textes = conseils
                .Where(c => c.Phase == phase)
                .OrderBy(c => c.Ordre)
                .Select(c => c.Texte)
                .ToArray();
            Phases.Add(new PhaseConseils
            {
                Kind = kind,
                Titre = TypeEtapeLabels.Libelle(PhaseDe(kind)),
                Textes = textes,
            });
        }
    }

    private static TypeEtape PhaseDe(StepKind kind) => kind switch
    {
        StepKind.Entretien => TypeEtape.Entretien,
        StepKind.Recolte => TypeEtape.Recolte,
        _ => TypeEtape.Semis,
    };

    private static string Capitaliser(string valeur) =>
        valeur.Length == 0
            ? valeur
            : char.ToUpperInvariant(valeur[0]) + valeur[1..];
}
