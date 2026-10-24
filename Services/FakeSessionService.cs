namespace CalendAgro.Services;

// Session locale simulée à partir du profil enregistré, sans réseau.
public sealed class FakeSessionService : ISessionService
{
    private readonly IProfilService _profil;

    public FakeSessionService(IProfilService profil) => _profil = profil;

    public async Task<StartInfo> GetStartAsync()
    {
        await Task.Delay(400);
        return new StartInfo(await _profil.GetProfilAsync() is not null);
    }
}
