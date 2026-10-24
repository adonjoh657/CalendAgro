namespace CalendAgro.Services;

public sealed class TabNavigationService : ITabNavigationService
{
    private static readonly string[] OrderedRoutes = ["accueil", "cultures", "calendrier", "parcelles"];
    private int _lastStep;

    public IReadOnlyList<string> TabRoutes => OrderedRoutes;

    public Task GoNextAsync() => GoAsync(+1);

    public Task GoPreviousAsync() => GoAsync(-1);

    public async Task GoHomeAsync()
    {
        var shell = Shell.Current;
        if (shell is null)
        {
            return;
        }

        var current = shell.CurrentState?.Location.OriginalString.Split('/').LastOrDefault();
        if (current == OrderedRoutes[0])
        {
            return;
        }

        _lastStep = -1;
        await PageSlideAnimator.SlideOutAsync((shell.CurrentPage as ContentPage)?.Content as VisualElement, -1);
        await shell.GoToAsync($"///{OrderedRoutes[0]}");
    }

    public int ConsumeLastStep()
    {
        var step = _lastStep;
        _lastStep = 0;
        return step;
    }

    private async Task GoAsync(int step)
    {
        var shell = Shell.Current;
        if (shell is null)
        {
            return;
        }

        var current = shell.CurrentState?.Location.OriginalString.Split('/').LastOrDefault();
        var index = Array.IndexOf(OrderedRoutes, current);
        if (index < 0)
        {
            return;
        }

        var target = index + step;
        if (target < 0 || target >= OrderedRoutes.Length)
        {
            return;
        }

        _lastStep = step;
        await PageSlideAnimator.SlideOutAsync((shell.CurrentPage as ContentPage)?.Content as VisualElement, step);
        await shell.GoToAsync($"///{OrderedRoutes[target]}");
    }
}
