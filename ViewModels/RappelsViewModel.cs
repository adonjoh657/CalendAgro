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

// Rappels de saison : liste des travaux à venir, activables un par un.
public sealed partial class RappelsViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IRappelService _rappels;

    public ObservableCollection<RappelCarte> Items { get; } = [];

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string ErreurBascule { get; set; } = string.Empty;

    public bool IsContenuVisible => !IsBusy && !HasError;

    public bool ListeVide => IsContenuVisible && Items.Count == 0;

    public bool ErreurBasculeVisible => ErreurBascule.Length > 0;

    public RappelsViewModel(IRappelService rappels) => _rappels = rappels;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public RappelsViewModel() : this(DefaultServices.Rappels)
    {
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsContenuVisible));
        OnPropertyChanged(nameof(ListeVide));
    }

    partial void OnHasErrorChanged(bool value)
    {
        OnPropertyChanged(nameof(IsContenuVisible));
        OnPropertyChanged(nameof(ListeVide));
    }

    partial void OnErreurBasculeChanged(string value) =>
        OnPropertyChanged(nameof(ErreurBasculeVisible));

    public Task LoadAsync() => RechargerAsync();

    [RelayCommand]
    private async Task RechargerAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            var rappels = await _rappels.GetRappelsAsync();
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

            OnPropertyChanged(nameof(ListeVide));
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

    private static StepKind BadgeDe(TypeEtape phase) => phase switch
    {
        TypeEtape.Entretien => StepKind.Entretien,
        TypeEtape.Recolte => StepKind.Recolte,
        _ => StepKind.Semis,
    };
}
