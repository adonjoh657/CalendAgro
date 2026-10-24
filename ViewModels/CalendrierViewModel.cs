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

// Calendrier mensuel : grille, jour sélectionné, ensuite.
public sealed partial class CalendrierViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IAccueilService _travaux;
    private readonly ITabNavigationService _tabs;
    private readonly List<Travail> _cache = [];
    private bool _started;
    private int _annee;
    private int _mois;
    private DateOnly _selection;

    public ObservableCollection<JourCalendrier> Jours { get; } = [];

    public ObservableCollection<TravailCarte> TachesJour { get; } = [];

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
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    public bool IsContenuVisible => !IsBusy && !HasError;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    public CalendrierViewModel(IAccueilService travaux, ITabNavigationService tabs)
    {
        _travaux = travaux;
        _tabs = tabs;
        var aujourdHui = DateOnly.FromDateTime(DateTime.Today);
        _annee = aujourdHui.Year;
        _mois = aujourdHui.Month;
        _selection = aujourdHui;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public CalendrierViewModel() : this(DefaultServices.Accueil, DefaultServices.Navigation)
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
            var donnees = await _travaux.GetAccueilAsync();
            _cache.Clear();
            _cache.AddRange(donnees.Travaux);
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
    private void ChoisirJour(DateOnly date)
    {
        _selection = date;
        ConstruireMois();
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
            var phases = _cache.Where(t => t.Date == date).Select(t => t.Phase).Distinct().ToArray();
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
        var travaux = _cache.Where(t => t.Date == _selection).OrderBy(t => t.Titre).ToArray();
        JourSelectionTitre = Capitaliser(_selection.ToString("dddd d MMMM", Francais));
        JourSelectionDetail = travaux.Length switch
        {
            0 => AppResources.Calendrier_AucuneTache,
            1 when _selection == DateOnly.FromDateTime(DateTime.Today) => AppResources.Calendrier_UneTache,
            _ => string.Format(AppResources.Calendrier_TachesJour, travaux.Length),
        };
        TachesJour.Clear();
        foreach (var travail in travaux)
        {
            TachesJour.Add(new TravailCarte
            {
                Id = travail.Id,
                Titre = travail.Titre,
                Parcelle = $"{travail.ParcelleNom} · {travail.CultureNom}",
                Jour = travail.Date.ToString("d MMM", Francais),
                Phase = TypeEtapeLabels.Libelle(travail.Phase),
                Kind = BadgeDe(travail.Phase),
            });
        }
        var suivantes = _cache
            .Where(t => t.Date > _selection)
            .OrderBy(t => t.Date)
            .Take(2)
            .Select(t => string.Format(
                AppResources.Calendrier_EnsuiteLigne,
                t.Date.ToString("d MMM", Francais),
                $"{Minuscule(t.Titre)} de {Minuscule(t.CultureNom)}"))
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
