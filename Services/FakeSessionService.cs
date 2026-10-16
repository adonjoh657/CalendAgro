namespace CalendAgro.Services;

// Données factices : aucun compte local, lecture simulée de 400 ms.
public sealed class FakeSessionService : ISessionService
{
    public async Task<StartInfo> GetStartAsync()
    {
        await Task.Delay(400);
        return new StartInfo(false, "Mireille");
    }
}
