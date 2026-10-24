using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Détail d'une date : travaux en cours (semis, entretien, récolte) et rappels.
[QueryProperty(nameof(DateChoisie), "date")]
public sealed partial class DetailJourViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IAccueilService _accueil;
    private readonly IRappelPersonnelService _rappels;
    private DateOnly _date = DateOnly.FromDateTime(DateTime.Today);

    public ObservableCollection<TravailCarte> Travaux { get; } = [];

    public ObservableCollection<RappelJour> Rappels { get; } = [];

    [ObservableProperty]
    public partial string TitreJour { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool TravauxVisible { get; set; }

    [ObservableProperty]
    public partial bool RappelsVisible { get; set; }

    [ObservableProperty]
    public partial bool AucunEvenement { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string NavigationErreur { get; set; } = string.Empty;

    public string DateChoisie
    {
        set
        {
            if (DateOnly.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                _date = date;
            }

            TitreJour = Capitaliser(_date.ToString("dddd d MMMM yyyy", Francais));
        }
    }

    public bool IsContenuVisible => !IsBusy && !HasError;

    public bool NavigationErreurVisible => NavigationErreur.Length > 0;

    public DetailJourViewModel(IAccueilService accueil, IRappelPersonnelService rappels)
    {
        _accueil = accueil;
        _rappels = rappels;
        TitreJour = Capitaliser(_date.ToString("dddd d MMMM yyyy", Francais));
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public DetailJourViewModel()
        : this(DefaultServices.Accueil, DefaultServices.RappelsPersonnels)
    {
    }

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsContenuVisible));

    partial void OnNavigationErreurChanged(string value) =>
        OnPropertyChanged(nameof(NavigationErreurVisible));

    public async Task LoadAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            var periodes = await _accueil.GetCalendrierAsync();
            var rappels = await _rappels.GetRappelsAsync();

            Travaux.Clear();
            foreach (var periode in periodes.Where(p => p.Contient(_date)))
            {
                Travaux.Add(new TravailCarte
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

            Rappels.Clear();
            foreach (var rappel in rappels
                .Where(r => r.Actif && RappelOccurrences.TombeLe(r, _date))
                .OrderBy(r => r.Heure)
                .ThenBy(r => r.Titre, StringComparer.CurrentCultureIgnoreCase))
            {
                Rappels.Add(new RappelJour
                {
                    Titre = rappel.Titre,
                    Detail = string.Format(
                        AppResources.Rappel_Detail,
                        rappel.Heure.ToString("HH:mm", Francais),
                        FrequenceRappelLabels.Libelle(rappel.Frequence)),
                });
            }

            TravauxVisible = Travaux.Count > 0;
            RappelsVisible = Rappels.Count > 0;
            AucunEvenement = !TravauxVisible && !RappelsVisible;
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
    private Task RechargerAsync() => LoadAsync();

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
    private async Task AjouterRappelAsync()
    {
        try
        {
            var date = _date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            await Shell.Current.GoToAsync($"edition-rappel?date={date}");
        }
        catch
        {
            NavigationErreur = AppResources.Rappel_NavigationErreur;
        }
    }

    [RelayCommand]
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    private static StepKind BadgeDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => StepKind.Entretien,
        TypeEtape.Recolte => StepKind.Recolte,
        _ => StepKind.Semis,
    };

    private static string Capitaliser(string valeur) =>
        valeur.Length == 0
            ? valeur
            : char.ToUpperInvariant(valeur[0]) + valeur[1..];
}
