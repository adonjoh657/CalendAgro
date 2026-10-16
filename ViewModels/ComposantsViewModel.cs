using CalendAgro.Controls;
using CalendAgro.Resources.Strings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CalendAgro.ViewModels;

// Page de démo des contrôles réutilisables.
public sealed partial class ComposantsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial StateKind LiveState { get; set; } = StateKind.Empty;

    public string LiveMessage => LiveState switch
    {
        StateKind.Loading => AppResources.Composants_EtatChargement,
        StateKind.Error => AppResources.Composants_EtatErreur,
        _ => AppResources.Composants_EtatVide,
    };

    partial void OnLiveStateChanged(StateKind value) => OnPropertyChanged(nameof(LiveMessage));

    [RelayCommand]
    private void CycleLiveState() => LiveState = (StateKind)(((int)LiveState + 1) % 3);

    [RelayCommand]
    private async Task RetryAsync()
    {
        LiveState = StateKind.Loading;
        await Task.Delay(900);
        LiveState = StateKind.Empty;
    }
}
