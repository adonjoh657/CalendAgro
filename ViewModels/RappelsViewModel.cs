using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Carte rappel : travail à venir + interrupteur activé / désactivé.
public sealed partial class RappelCarte : ObservableObject
{
    private readonly Func<string, bool, Task> _enregistrer;
    private bool _pret;

    public RappelCarte(
        string id,
        string titre,
        string parcelle,
        string date,
        string phase,
        StepKind kind,
        bool actif,
        Func<string, bool, Task> enregistrer)
    {
        Id = id;
        Titre = titre;
        Parcelle = parcelle;
        Date = date;
        Phase = phase;
        Kind = kind;
        _enregistrer = enregistrer;
        Actif = actif;
        _pret = true;
    }

    public string Id { get; }

    public string Titre { get; }

    public string Parcelle { get; }

    public string Date { get; }

    public string Phase { get; }

    public StepKind Kind { get; }

    [ObservableProperty]
    public partial bool Actif { get; set; }

    public string EtatLibelle => Actif
        ? AppResources.Rappels_Active
        : AppResources.Rappels_Desactive;

    public string DescriptionBascule => string.Format(AppResources.Rappels_Basculer, Titre);

    partial void OnActifChanged(bool value)
    {
        OnPropertyChanged(nameof(EtatLibelle));
        if (_pret)
        {
            _ = _enregistrer(Id, value);
        }
    }
}

// Rappel personnel : fréquence, activation et suppression.
public sealed partial class RappelPersonnelCarte : ObservableObject
{
    private readonly Func<RappelPersonnelCarte, bool, Task> _basculer;
    private readonly Func<RappelPersonnelCarte, Task> _supprimer;
    private bool _pret;

    public RappelPersonnelCarte(
        RappelPersonnel rappel,
        string detail,
        Func<RappelPersonnelCarte, bool, Task> basculer,
        Func<RappelPersonnelCarte, Task> supprimer)
    {
        Rappel = rappel;
        Id = rappel.Id;
        Titre = rappel.Titre;
        Detail = detail;
        _basculer = basculer;
        _supprimer = supprimer;
        Actif = rappel.Actif;
        _pret = true;
    }

    public RappelPersonnel Rappel { get; }

    public string Id { get; }

    public string Titre { get; }

    public string Detail { get; }

    [ObservableProperty]
    public partial bool Actif { get; set; }

    public string EtatLibelle => Actif
        ? AppResources.Rappels_Active
        : AppResources.Rappels_Desactive;

    public string DescriptionBascule => string.Format(AppResources.Rappels_Basculer, Titre);

    public string DescriptionSuppression =>
        string.Format(AppResources.Rappels_SupprimerDescription, Titre);

    partial void OnActifChanged(bool value)
    {
        OnPropertyChanged(nameof(EtatLibelle));
        if (_pret)
        {
            _ = _basculer(this, value);
        }
    }

    [RelayCommand]
    private Task SupprimerAsync() => _supprimer(this);
}

// Rappels de saison : liste des travaux à venir, activables un par un.
public sealed partial class RappelsViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IRappelService _rappels;
    private readonly IRappelPersonnelService _rappelsPersonnels;
    private readonly IRappelNotificationService _notifications;

    public ObservableCollection<RappelCarte> Items { get; } = [];

    public ObservableCollection<RappelPersonnelCarte> Personnels { get; } = [];

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string ErreurBascule { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErreurSuppression { get; set; } = string.Empty;

    public bool IsContenuVisible => !IsBusy && !HasError;

    public bool ListeVide => IsContenuVisible && Items.Count == 0 && Personnels.Count == 0;

    public bool PersonnelsVisible => IsContenuVisible && Personnels.Count > 0;

    public bool SaisonVisible => IsContenuVisible && Items.Count > 0;

    public bool ErreurBasculeVisible => ErreurBascule.Length > 0;

    public bool ErreurSuppressionVisible => ErreurSuppression.Length > 0;

    public RappelsViewModel(
        IRappelService rappels,
        IRappelPersonnelService rappelsPersonnels,
        IRappelNotificationService notifications)
    {
        _rappels = rappels;
        _rappelsPersonnels = rappelsPersonnels;
        _notifications = notifications;
    }

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public RappelsViewModel()
        : this(
            DefaultServices.Rappels,
            DefaultServices.RappelsPersonnels,
            DefaultServices.Notifications)
    {
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsContenuVisible));
        OnPropertyChanged(nameof(ListeVide));
        OnPropertyChanged(nameof(PersonnelsVisible));
        OnPropertyChanged(nameof(SaisonVisible));
    }

    partial void OnHasErrorChanged(bool value)
    {
        OnPropertyChanged(nameof(IsContenuVisible));
        OnPropertyChanged(nameof(ListeVide));
        OnPropertyChanged(nameof(PersonnelsVisible));
        OnPropertyChanged(nameof(SaisonVisible));
    }

    partial void OnErreurBasculeChanged(string value) =>
        OnPropertyChanged(nameof(ErreurBasculeVisible));

    partial void OnErreurSuppressionChanged(string value) =>
        OnPropertyChanged(nameof(ErreurSuppressionVisible));

    public Task LoadAsync() => RechargerAsync();

    [RelayCommand]
    private async Task RechargerAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            var rappels = await _rappels.GetRappelsAsync();
            var personnels = await _rappelsPersonnels.GetRappelsAsync();
            Items.Clear();
            foreach (var rappel in rappels)
            {
                Items.Add(new RappelCarte(
                    rappel.Id,
                    rappel.Titre,
                    $"{rappel.ParcelleNom} · {rappel.CultureNom}",
                    rappel.Date.ToString("d MMMM yyyy", Francais),
                    TypeEtapeLabels.Libelle(rappel.Phase),
                    BadgeDe(rappel.Phase),
                    rappel.Actif,
                    EnregistrerActifAsync));
            }

            Personnels.Clear();
            foreach (var rappel in personnels)
            {
                Personnels.Add(new RappelPersonnelCarte(
                    rappel,
                    string.Format(
                        AppResources.Rappel_Detail,
                        FrequenceRappelLabels.Libelle(rappel.Frequence),
                        $"{rappel.DateDebut.ToString("d MMM yyyy", Francais)} · {rappel.Heure.ToString("HH:mm", Francais)}"),
                    BasculerPersonnelAsync,
                    SupprimerPersonnelAsync));
            }

            OnPropertyChanged(nameof(ListeVide));
            OnPropertyChanged(nameof(PersonnelsVisible));
            OnPropertyChanged(nameof(SaisonVisible));
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
    private Task AjouterParcelleAsync() => Shell.Current.GoToAsync("ajout-parcelle");

    [RelayCommand]
    private Task RetourAsync() => Shell.Current.GoToAsync("..");

    private async Task EnregistrerActifAsync(string rappelId, bool actif)
    {
        try
        {
            await _rappels.SetActifAsync(rappelId, actif);
            ErreurBascule = string.Empty;
        }
        catch
        {
            ErreurBascule = AppResources.Rappels_ErreurBascule;
        }
    }

    private async Task BasculerPersonnelAsync(RappelPersonnelCarte carte, bool actif)
    {
        try
        {
            await _rappelsPersonnels.SetActifAsync(carte.Id, actif);
            carte.Rappel.Actif = actif;
            if (actif)
            {
                await _notifications.DemanderPermissionAsync();
            }

            await _notifications.PlanifierAsync(carte.Rappel);
            ErreurBascule = string.Empty;
        }
        catch
        {
            ErreurBascule = AppResources.Rappels_ErreurBascule;
        }
    }

    private async Task SupprimerPersonnelAsync(RappelPersonnelCarte carte)
    {
        ErreurSuppression = string.Empty;
        try
        {
            var confirme = await Shell.Current.DisplayAlertAsync(
                AppResources.Rappels_SupprimerTitre,
                AppResources.Rappels_SupprimerMessage,
                AppResources.Rappels_SupprimerOui,
                AppResources.Rappels_SupprimerNon);
            if (!confirme)
            {
                return;
            }

            await _rappelsPersonnels.SupprimerAsync(carte.Id);
            _notifications.Annuler(carte.Id);
            Personnels.Remove(carte);
            OnPropertyChanged(nameof(ListeVide));
            OnPropertyChanged(nameof(PersonnelsVisible));
            OnPropertyChanged(nameof(SaisonVisible));
        }
        catch
        {
            ErreurSuppression = AppResources.Rappels_SuppressionErreur;
        }
    }

    private static StepKind BadgeDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => StepKind.Entretien,
        TypeEtape.Recolte => StepKind.Recolte,
        _ => StepKind.Semis,
    };
}
