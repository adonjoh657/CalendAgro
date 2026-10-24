using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Création d'un rappel personnel : titre, première date, heure et fréquence.
// La date arrive depuis le calendrier (jour sélectionné).
[QueryProperty(nameof(DateDemandee), "date")]
public sealed partial class EditionRappelViewModel : ObservableObject
{
    private const int LongueurTitreMinimum = 2;
    private const int LongueurTitreMaximum = 40;

    private static readonly FrequenceRappel[] OrdreFrequences =
    [
        FrequenceRappel.UneFois,
        FrequenceRappel.Quotidienne,
        FrequenceRappel.Hebdomadaire,
        FrequenceRappel.Mensuelle,
    ];

    private readonly IRappelPersonnelService _rappels;
    private readonly IRappelNotificationService _notifications;

    public ObservableCollection<string> Frequences { get; } = [];

    [ObservableProperty]
    public partial string Titre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime DateDebut { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial TimeSpan Heure { get; set; } = new(8, 0, 0);

    [ObservableProperty]
    public partial string? FrequenceChoisie { get; set; }

    [ObservableProperty]
    public partial string TitreErreur { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool DateInvalide { get; set; }

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    [ObservableProperty]
    public partial string Erreur { get; set; } = string.Empty;

    public string DateDemandee
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
                DateDebut = date.ToDateTime(TimeOnly.MinValue);
            }
        }
    }

    public bool TitreErreurVisible => TitreErreur.Length > 0;

    public bool ErreurVisible => Erreur.Length > 0;

    public EditionRappelViewModel(IRappelPersonnelService rappels, IRappelNotificationService notifications)
    {
        _rappels = rappels;
        _notifications = notifications;
        foreach (var frequence in OrdreFrequences)
        {
            Frequences.Add(FrequenceRappelLabels.Libelle(frequence));
        }

        FrequenceChoisie = Frequences[0];
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public EditionRappelViewModel()
        : this(DefaultServices.RappelsPersonnels, DefaultServices.Notifications)
    {
    }

    partial void OnTitreChanged(string value)
    {
        var longueur = value.Trim().Length;
        TitreErreur = longueur is >= LongueurTitreMinimum and <= LongueurTitreMaximum
            ? string.Empty
            : AppResources.Rappel_TitreErreur;
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnTitreErreurChanged(string value) => OnPropertyChanged(nameof(TitreErreurVisible));

    partial void OnDateDebutChanged(DateTime value) => ActualiserDate();

    partial void OnHeureChanged(TimeSpan value) => ActualiserDate();

    partial void OnFrequenceChoisieChanged(string? value) => ActualiserDate();

    partial void OnIsSavingChanged(bool value) => EnregistrerCommand.NotifyCanExecuteChanged();

    partial void OnErreurChanged(string value) => OnPropertyChanged(nameof(ErreurVisible));

    [RelayCommand(CanExecute = nameof(PeutEnregistrer))]
    private async Task EnregistrerAsync()
    {
        IsSaving = true;
        Erreur = string.Empty;
        try
        {
            var frequence = OrdreFrequences[Math.Max(0, Frequences.IndexOf(FrequenceChoisie ?? string.Empty))];
            var rappel = await _rappels.AjouterAsync(
                Titre,
                DateOnly.FromDateTime(DateDebut),
                TimeOnly.FromTimeSpan(Heure),
                frequence);

            // L'enregistrement local est acquis : une permission refusée ne doit pas le perdre.
            await _notifications.DemanderPermissionAsync();
            await _notifications.PlanifierAsync(rappel);
            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            Erreur = AppResources.Rappel_Erreur;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    private bool PeutEnregistrer() =>
        !IsSaving
        && TitreErreur.Length == 0
        && Titre.Trim().Length > 0
        && !DateInvalide
        && FrequenceChoisie is not null;

    private void ActualiserDate()
    {
        DateInvalide = FrequenceChoisie == Frequences[0]
            && DateDebut.Date.Add(Heure) <= DateTime.Now;
        EnregistrerCommand.NotifyCanExecuteChanged();
    }
}
