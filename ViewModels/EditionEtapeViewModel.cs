using System.Collections.ObjectModel;
using System.Globalization;
using CalendAgro.Models;
using CalendAgro.Resources.Strings;
using CalendAgro.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Champ de saisie d'un conseil, avec son message d'erreur (12 mots au maximum).
public sealed partial class ConseilSaisie : ObservableObject
{
    [ObservableProperty]
    public partial string Libelle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Texte { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Erreur { get; set; } = string.Empty;

    public bool ErreurVisible => Erreur.Length > 0;

    public bool EstValide => ConseilRegles.EstValide(Texte);

    public event EventHandler? Modifie;

    partial void OnTexteChanged(string value)
    {
        var mots = ConseilRegles.CompterMots(value);
        Erreur = mots > ConseilRegles.MotsMax
            ? string.Format(AppResources.Etape_ConseilErreur, mots)
            : string.Empty;
        Modifie?.Invoke(this, EventArgs.Empty);
    }

    partial void OnErreurChanged(string value) => OnPropertyChanged(nameof(ErreurVisible));
}

// Saisie d'une étape (période d'une phase pour une zone et une saison) et des
// conseils de cette phase : semis, entretien ou récolte d'une culture.
[QueryProperty(nameof(CultureId), "cultureId")]
[QueryProperty(nameof(PhaseNom), "phase")]
[QueryProperty(nameof(Zone), "zone")]
[QueryProperty(nameof(Saison), "saison")]
public sealed partial class EditionEtapeViewModel : ObservableObject
{
    private const int NombreConseils = 4;
    private const int MoisParAn = 12;

    private readonly ICultureService _cultures;
    private string _cultureId = string.Empty;
    private string _phaseNom = string.Empty;
    private string _zone = string.Empty;
    private string _saison = string.Empty;
    private TypeEtape _phase = TypeEtape.Semis;

    public IReadOnlyList<string> Mois { get; } = NomsDesMois();

    public ObservableCollection<ConseilSaisie> Conseils { get; } = [];

    [ObservableProperty]
    public partial string Titre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int DebutIndex { get; set; } = -1;

    [ObservableProperty]
    public partial int FinIndex { get; set; } = -1;

    [ObservableProperty]
    public partial bool EtapeExiste { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; } = true;

    [ObservableProperty]
    public partial bool HasError { get; set; }

    public string CultureId
    {
        get => _cultureId;
        set
        {
            _cultureId = Uri.UnescapeDataString(value ?? string.Empty);
            DemanderChargement();
        }
    }

    public string PhaseNom
    {
        get => _phaseNom;
        set
        {
            _phaseNom = Uri.UnescapeDataString(value ?? string.Empty);
            DemanderChargement();
        }
    }

    public string Zone
    {
        get => _zone;
        set
        {
            _zone = Uri.UnescapeDataString(value ?? string.Empty);
            DemanderChargement();
        }
    }

    public string Saison
    {
        get => _saison;
        set
        {
            _saison = Uri.UnescapeDataString(value ?? string.Empty);
            DemanderChargement();
        }
    }

    public bool IsFormVisible => !IsBusy && !HasError;

    public bool DebutErreurVisible => DebutIndex < 0 && !IsBusy;

    public bool FinErreurVisible => FinIndex < 0 && !IsBusy;

    public EditionEtapeViewModel(ICultureService cultures) => _cultures = cultures;

    // Repli hors DI (Shell DataTemplate = constructeur sans paramètre).
    public EditionEtapeViewModel() : this(DefaultServices.Cultures)
    {
    }

    partial void OnDebutIndexChanged(int value)
    {
        OnPropertyChanged(nameof(DebutErreurVisible));
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnFinIndexChanged(int value)
    {
        OnPropertyChanged(nameof(FinErreurVisible));
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFormVisible));
        OnPropertyChanged(nameof(DebutErreurVisible));
        OnPropertyChanged(nameof(FinErreurVisible));
        EnregistrerCommand.NotifyCanExecuteChanged();
    }

    partial void OnHasErrorChanged(bool value) => OnPropertyChanged(nameof(IsFormVisible));

    // Les paramètres de navigation arrivent un par un : on charge quand ils sont tous là.
    private void DemanderChargement()
    {
        if (_cultureId.Length > 0 && _phaseNom.Length > 0 && _zone.Length > 0 && _saison.Length > 0)
        {
            _ = ChargerAsync();
        }
    }

    private async Task ChargerAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            if (!Enum.TryParse(_phaseNom, out _phase))
            {
                HasError = true;
                return;
            }

            var culture = await _cultures.GetCultureAsync(_cultureId);
            if (culture is null)
            {
                HasError = true;
                return;
            }

            var phaseLibelle = TypeEtapeLabels.Libelle(_phase);
            Titre = string.Format(AppResources.Etape_TitreEdition, phaseLibelle);

            var etape = (await _cultures.GetEtapesAsync(_cultureId, _zone, _saison))
                .FirstOrDefault(e => e.Type == _phase);
            EtapeExiste = etape is not null;
            DebutIndex = etape is null ? -1 : etape.MoisDebut - 1;
            FinIndex = etape is null ? -1 : etape.MoisFin - 1;

            var existants = (await _cultures.GetConseilsAsync(_cultureId))
                .Where(c => c.Phase == _phase)
                .OrderBy(c => c.Ordre)
                .Select(c => c.Texte)
                .ToArray();
            RemplirConseils(existants);
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

    private void RemplirConseils(IReadOnlyList<string> existants)
    {
        foreach (var ancien in Conseils)
        {
            ancien.Modifie -= OnConseilModifie;
        }

        Conseils.Clear();
        for (var i = 0; i < NombreConseils; i++)
        {
            var saisie = new ConseilSaisie
            {
                Libelle = string.Format(AppResources.Etape_ConseilN, i + 1),
                Texte = i < existants.Count ? existants[i] : string.Empty,
            };
            saisie.Modifie += OnConseilModifie;
            Conseils.Add(saisie);
        }
    }

    private void OnConseilModifie(object? sender, EventArgs e) => EnregistrerCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(PeutEnregistrer))]
    private async Task EnregistrerAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            await _cultures.EnregistrerEtapeAsync(
                _cultureId,
                _phase,
                _zone,
                _saison,
                DebutIndex + 1,
                FinIndex + 1);
            await _cultures.EnregistrerConseilsAsync(
                _cultureId,
                _phase,
                Conseils.Select(c => c.Texte).ToArray());
            await Shell.Current.GoToAsync("..");
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
    private async Task SupprimerEtapeAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            await _cultures.SupprimerEtapeAsync(_cultureId, _phase, _zone, _saison);
            await Shell.Current.GoToAsync("..");
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

    private bool PeutEnregistrer() =>
        !IsBusy
        && DebutIndex is >= 0 and < MoisParAn
        && FinIndex is >= 0 and < MoisParAn
        && Conseils.All(c => c.EstValide);

    private static string[] NomsDesMois()
    {
        var noms = CultureInfo.GetCultureInfo("fr-FR").DateTimeFormat.MonthNames;
        return noms
            .Take(MoisParAn)
            .Select(n => n.Length == 0 ? n : char.ToUpperInvariant(n[0]) + n[1..])
            .ToArray();
    }
}
