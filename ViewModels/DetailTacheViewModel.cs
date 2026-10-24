using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Controls;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Détail d'une tâche : date, parcelle, conseils, marquage terminé.
[QueryProperty(nameof(TravailId), "travailId")]
public sealed partial class DetailTacheViewModel : ObservableObject
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IAccueilService _accueil;
    private string _travailId = string.Empty;

    public ObservableCollection<ConseilBloc> Conseils { get; } = [];

    [ObservableProperty]
    public partial string Titre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StepKind Kind { get; set; } = StepKind.Entretien;

    [ObservableProperty]
    public partial string Phase { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool EstTerminee { get; set; }

    [ObservableProperty]
    public partial string DatePrevue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ParcelleNom { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ParcelleMeta { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial bool Introuvable { get; set; }

    public string TravailId
    {
        get => _travailId;
        set
        {
            _travailId = Uri.UnescapeDataString(value ?? string.Empty);
            _ = RechargerAsync();
        }
    }

    public string Statut => EstTerminee ? AppResources.Detail_Terminee : AppResources.Detail_AFaire;

    public bool IsContenuVisible => !IsBusy && !HasError && !Introuvable;

    public DetailTacheViewModel(IAccueilService accueil) => _accueil = accueil;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public DetailTacheViewModel() : this(DefaultServices.Accueil)
    {
    }

    partial void OnEstTermineeChanged(bool value)
    {
        OnPropertyChanged(nameof(Statut));
        MarquerTermineeCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsContenuVisible));
        MarquerTermineeCommand.NotifyCanExecuteChanged();
    }

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
            var detail = await _accueil.GetDetailAsync(_travailId);
            if (detail is null)
            {
                Introuvable = true;
                return;
            }
            Titre = detail.Titre;
            Kind = BadgeDe(detail.Phase);
            Phase = TypeEtapeLabels.Libelle(detail.Phase);
            EstTerminee = detail.Terminee;
            DatePrevue = Capitaliser(detail.DatePrevue.ToString("dddd d MMMM yyyy", Francais));
            ParcelleNom = detail.ParcelleNom;
            ParcelleMeta = $"{detail.CultureNom} · {string.Format(AppResources.Accueil_Hectares, detail.SurfaceHa.ToString("0.00", Francais))}";
            Conseils.Clear();
            foreach (var conseil in detail.Conseils)
            {
                Conseils.Add(conseil);
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

    [RelayCommand(CanExecute = nameof(PeutTerminer))]
    private async Task MarquerTermineeAsync()
    {
        IsBusy = true;
        try
        {
            await _accueil.MarquerTermineeAsync(_travailId);
            EstTerminee = true;
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

    private bool PeutTerminer() => !IsBusy && !EstTerminee;

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
