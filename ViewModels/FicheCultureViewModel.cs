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

// Fiche culture : frise 12 mois, zone et saison.
[QueryProperty(nameof(CultureId), "cultureId")]
public sealed partial class FicheCultureViewModel : ObservableObject
{
    private readonly ICultureService _cultures;
    private readonly IParcelleService _parcelles;
    private string _cultureId = string.Empty;

    public ObservableCollection<MoisLigne> Mois { get; } = [];

    public ObservableCollection<string> Zones { get; } = [];

    public ObservableCollection<string> Saisons { get; } = [];

    [ObservableProperty]
    public partial string? ZoneChoisie { get; set; }

    [ObservableProperty]
    public partial string? SaisonChoisie { get; set; }

    [ObservableProperty]
    public partial string Nom { get; set; } = string.Empty;

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

    public FicheCultureViewModel(ICultureService cultures, IParcelleService parcelles)
    {
        _cultures = cultures;
        _parcelles = parcelles;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public FicheCultureViewModel() : this(DefaultServices.Cultures, DefaultServices.Parcelles)
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
            _ = ChangerZoneAsync();
        }
    }

    partial void OnSaisonChoisieChanged(string? value)
    {
        if (value is not null && !IsBusy)
        {
            _ = RechargerCalendrierAsync();
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

            await ChargerZonesAsync();
            if (ZoneChoisie is null || !Zones.Contains(ZoneChoisie))
            {
                ZoneChoisie = Zones.FirstOrDefault();
            }

            await ChargerSaisonsAsync(ZoneChoisie);
            if (SaisonChoisie is null || !Saisons.Contains(SaisonChoisie))
            {
                SaisonChoisie = Saisons.FirstOrDefault();
            }

            await RemplirCalendrierAsync();
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

    // La région change : ses saisons changent aussi, on repart de la première.
    private async Task ChangerZoneAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            await ChargerSaisonsAsync(ZoneChoisie);
            SaisonChoisie = Saisons.FirstOrDefault();
            await RemplirCalendrierAsync();
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

    private async Task RechargerCalendrierAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            await RemplirCalendrierAsync();
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

    private async Task RemplirCalendrierAsync()
    {
        if (ZoneChoisie is null || SaisonChoisie is null)
        {
            return;
        }

        Region = string.Format(AppResources.Fiche_RegionExemple, ZoneChoisie);
        var etapes = await _cultures.GetEtapesAsync(_cultureId, ZoneChoisie, SaisonChoisie);
        RemplirMois(etapes);
    }

    [RelayCommand]
    private async Task OuvrirOptionsAsync()
    {
        var choix = await Shell.Current.DisplayActionSheetAsync(
            AppResources.Fiche_Options,
            AppResources.Fiche_SupprimerNon,
            AppResources.Fiche_Supprimer,
            AppResources.Fiche_ModifierCulture,
            AppResources.Fiche_ModifierSemis,
            AppResources.Fiche_ModifierEntretien,
            AppResources.Fiche_ModifierRecolte);

        if (choix == AppResources.Fiche_ModifierCulture)
        {
            await ModifierCultureAsync();
        }
        else if (choix == AppResources.Fiche_ModifierSemis)
        {
            await OuvrirEditionEtapeAsync(TypeEtape.Semis);
        }
        else if (choix == AppResources.Fiche_ModifierEntretien)
        {
            await OuvrirEditionEtapeAsync(TypeEtape.Entretien);
        }
        else if (choix == AppResources.Fiche_ModifierRecolte)
        {
            await OuvrirEditionEtapeAsync(TypeEtape.Recolte);
        }
        else if (choix == AppResources.Fiche_Supprimer)
        {
            await SupprimerAsync();
        }
    }

    private async Task ModifierCultureAsync()
    {
        try
        {
            await Shell.Current.GoToAsync($"edition-culture?cultureId={Uri.EscapeDataString(_cultureId)}");
        }
        catch
        {
            NavigationErreur = AppResources.Connexion_ErreurLien;
        }
    }

    // Ouvre la saisie de la période et des conseils d'une phase, pour la zone et la saison choisies.
    private async Task OuvrirEditionEtapeAsync(TypeEtape phase)
    {
        try
        {
            await Shell.Current.GoToAsync(
                $"edition-etape?cultureId={Uri.EscapeDataString(_cultureId)}&phase={phase}" +
                $"&zone={Uri.EscapeDataString(ZoneChoisie ?? string.Empty)}" +
                $"&saison={Uri.EscapeDataString(SaisonChoisie ?? string.Empty)}");
        }
        catch
        {
            NavigationErreur = AppResources.Connexion_ErreurLien;
        }
    }

    private async Task SupprimerAsync()
    {
        NavigationErreur = string.Empty;
        try
        {
            var utilisee = (await _parcelles.GetParcellesAsync())
                .Any(p => string.Equals(p.CultureId, _cultureId, StringComparison.Ordinal));
            if (utilisee)
            {
                NavigationErreur = AppResources.Fiche_SuppressionImpossible;
                return;
            }

            var confirme = await Shell.Current.DisplayAlertAsync(
                AppResources.Fiche_SupprimerTitre,
                AppResources.Fiche_SupprimerMessage,
                AppResources.Fiche_SupprimerOui,
                AppResources.Fiche_SupprimerNon);
            if (!confirme)
            {
                return;
            }

            await _cultures.SupprimerCultureAsync(_cultureId);
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            NavigationErreur = AppResources.Fiche_SuppressionErreur;
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
            await Shell.Current.GoToAsync($"ajout-parcelle?culture={Uri.EscapeDataString(Nom)}");
        }
        catch
        {
            NavigationErreur = AppResources.Fiche_ParcelleEnCours;
        }
    }

    private async Task ChargerZonesAsync()
    {
        if (Zones.Count > 0)
        {
            return;
        }

        foreach (var zone in await _cultures.GetZonesAsync())
        {
            Zones.Add(zone);
        }
    }

    private async Task ChargerSaisonsAsync(string? zone)
    {
        Saisons.Clear();
        if (zone is null)
        {
            return;
        }

        foreach (var saison in await _cultures.GetSaisonsAsync(zone))
        {
            Saisons.Add(saison);
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
