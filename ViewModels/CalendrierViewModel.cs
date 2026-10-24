using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Case du mois : vide avant le 1er, pastilles de phases sinon.
public sealed class JourCalendrier
{
    public DateOnly Date { get; init; }

    public int Numero { get; init; }

    public bool EstVide { get; init; }

    public bool EstJour => !EstVide;

    public bool EstSelectionne { get; init; }

    public Color Fond { get; init; } = Colors.Transparent;

    public Color Texte { get; init; } = Colors.Black;

    public Color Bordure { get; init; } = Colors.Transparent;

    public IReadOnlyList<Color> Points { get; init; } = [];
}

// Rappel personnel affiché sur le jour sélectionné.
public sealed class RappelJour
{
    public string Titre { get; init; } = string.Empty;

    public string Detail { get; init; } = string.Empty;
}

// Calendrier mensuel : grille, jour sélectionné, ensuite.
public sealed partial class CalendrierViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IAccueilService _travaux;
    private readonly IRappelPersonnelService _rappelsPersonnels;
    private readonly ITabNavigationService _tabs;
    private readonly List<PeriodeTravail> _cache = [];
    private readonly List<RappelPersonnel> _rappelsCache = [];
    private bool _started;
    private int _annee;
    private int _mois;
    private DateOnly _selection;

    public ObservableCollection<JourCalendrier> Jours { get; } = [];

    public ObservableCollection<TravailCarte> TachesJour { get; } = [];

    public ObservableCollection<RappelJour> RappelsJour { get; } = [];

    public ObservableCollection<string> Ensuite { get; } = [];

    public IReadOnlyList<string> JoursSemaine { get; } =
        AppResources.Calendrier_JoursLettres.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    [ObservableProperty]
    public partial string TitreMois { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string JourSelectionTitre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string JourSelectionDetail { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool EnsuiteVisible { get; set; }

    [ObservableProperty]
    public partial bool RappelsJourVisible { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    public bool IsContenuVisible => !IsBusy && !HasError;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    public CalendrierViewModel(
        IAccueilService travaux,
        IRappelPersonnelService rappelsPersonnels,
        ITabNavigationService tabs)
    {
        _travaux = travaux;
        _rappelsPersonnels = rappelsPersonnels;
        _tabs = tabs;
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        _annee = aujourdHui.Year;
        _mois = aujourdHui.Month;
        _selection = aujourdHui;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public CalendrierViewModel()
        : this(DefaultServices.Accueil, DefaultServices.RappelsPersonnels, DefaultServices.Navigation)
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

    private async Task RechargerAsync(bool avecSquelette)
    {
        if (avecSquelette)
        {
            IsBusy = true;
        }
        HasError = false;
        try
        {
            var periodes = await _travaux.GetCalendrierAsync();
            var rappels = await _rappelsPersonnels.GetRappelsAsync();
            _cache.Clear();
            _cache.AddRange(periodes);
            _rappelsCache.Clear();
            _rappelsCache.AddRange(rappels);
            ConstruireMois();
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
    private void MoisPrecedent()
    {
        var date = new DateOnly(_annee, _mois, 1).AddMonths(-1);
        _annee = date.Year;
        _mois = date.Month;
        ConstruireMois();
    }

    [RelayCommand]
    private void MoisSuivant()
    {
        var date = new DateOnly(_annee, _mois, 1).AddMonths(1);
        _annee = date.Year;
        _mois = date.Month;
        ConstruireMois();
    }

    [RelayCommand]
    private async Task ChoisirJourAsync(DateOnly date)
    {
        _selection = date;
        ConstruireMois();
        try
        {
            var valeur = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            await Shell.Current.GoToAsync($"detail-jour?date={valeur}");
        }
        catch
        {
            NavigationErreur = AppResources.Calendrier_NavigationErreur;
        }
    }

    [RelayCommand]
    private async Task AjouterRappelAsync()
    {
        try
        {
            var date = _selection.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            await Shell.Current.GoToAsync($"edition-rappel?date={date}");
        }
        catch
        {
            NavigationErreur = AppResources.Rappel_NavigationErreur;
        }
    }

    [RelayCommand]
    private async Task OuvrirDetailAsync(string travailId)
    {
        try
        {
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

    private void ConstruireMois()
    {
        var premier = new DateOnly(_annee, _mois, 1);
        TitreMois = Capitaliser(premier.ToString("MMMM yyyy", Francais));
        var decalage = ((int)premier.DayOfWeek + 6) % 7;
        var joursDansMois = DateTime.DaysInMonth(_annee, _mois);
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        Jours.Clear();
        for (var i = 0; i < decalage; i++)
        {
            Jours.Add(new JourCalendrier { EstVide = true });
        }
        for (var j = 1; j <= joursDansMois; j++)
        {
            var date = new DateOnly(_annee, _mois, j);
            var phases = _cache.Where(p => p.Contient(date)).Select(p => p.Phase).Distinct().ToArray();
            var selectionne = date == _selection;
            Jours.Add(new JourCalendrier
            {
                Date = date,
                Numero = j,
                EstSelectionne = selectionne,
                Fond = Couleur(selectionne ? "Primary" : null),
                Texte = Couleur(selectionne ? "OnPrimary" : "Text"),
                Bordure = date == aujourdHui && !selectionne ? Couleur("Primary") : Colors.Transparent,
                Points = phases.Select(CouleurPhase).ToArray(),
            });
        }
        RafraichirJour();
    }

    private void RafraichirJour()
    {
        var periodes = _cache.Where(p => p.Contient(_selection)).ToArray();
        var rappels = _rappelsCache
            .Where(r => r.Actif && RappelOccurrences.TombeLe(r, _selection))
            .OrderBy(r => r.Heure)
            .ThenBy(r => r.Titre, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        JourSelectionTitre = Capitaliser(_selection.ToString("dddd d MMMM", Francais));
        var total = periodes.Length + rappels.Length;
        JourSelectionDetail = total switch
        {
            0 => AppResources.Calendrier_AucunElement,
            1 => AppResources.Calendrier_UnElement,
            _ => string.Format(AppResources.Calendrier_ElementsJour, total),
        };
        TachesJour.Clear();
        foreach (var periode in periodes)
        {
            TachesJour.Add(new TravailCarte
            {
                Id = periode.Id,
                Titre = periode.Titre,
                Parcelle = $"{periode.ParcelleNom} · {periode.CultureNom}",
                Jour = periode.Debut == periode.Fin
                    ? periode.Debut.ToString("d MMM", Francais)
                    : string.Format(
                        AppResources.Calendrier_Periode,
                        periode.Debut.ToString("d MMM", Francais),
                        periode.Fin.ToString("d MMM", Francais)),
                Phase = TypeEtapeLabels.Libelle(periode.Phase),
                Kind = BadgeDe(periode.Phase),
            });
        }
        RappelsJour.Clear();
        foreach (var rappel in rappels)
        {
            RappelsJour.Add(new RappelJour
            {
                Titre = rappel.Titre,
                Detail = string.Format(
                    AppResources.Rappel_Detail,
                    rappel.Heure.ToString("HH:mm", Francais),
                    FrequenceRappelLabels.Libelle(rappel.Frequence)),
            });
        }
        RappelsJourVisible = RappelsJour.Count > 0;
        var suivantes = _cache
            .Where(p => p.Debut > _selection)
            .OrderBy(p => p.Debut)
            .Take(2)
            .Select(p => string.Format(
                AppResources.Calendrier_EnsuiteLigne,
                p.Debut.ToString("d MMM", Francais),
                $"{Minuscule(p.Titre)} de {Minuscule(p.CultureNom)}"))
            .ToArray();
        Ensuite.Clear();
        foreach (var ligne in suivantes)
        {
            Ensuite.Add(ligne);
        }
        EnsuiteVisible = Ensuite.Count > 0;
    }

    private static StepKind BadgeDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => StepKind.Entretien,
        TypeEtape.Recolte => StepKind.Recolte,
        _ => StepKind.Semis,
    };

    private static Color Couleur(string? cle) =>
        cle is not null
        && Application.Current?.Resources.TryGetValue(cle, out var valeur) == true
        && valeur is Color couleur
            ? couleur
            : Colors.Transparent;

    private static Color CouleurPhase(TypeEtape phase) => Couleur(phase switch
    {
        TypeEtape.Entretien => "Entretien",
        TypeEtape.Recolte => "Recolte",
        _ => "Semis",
    });

    private static string Capitaliser(string valeur) =>
        valeur.Length == 0
            ? valeur
            : char.ToUpperInvariant(valeur[0]) + valeur[1..];

    private static string Minuscule(string valeur) =>
        valeur.Length == 0
            ? valeur
            : char.ToLowerInvariant(valeur[0]) + valeur[1..];
}
