namespace CalendAgro.Services;

// Session locale : compte existant ou usage sans compte.
// Implémentation réelle plus tard ; factice pour la phase B.
public sealed record StartInfo(bool HasLocalAccount, string UserName);

public interface ISessionService
{
    Task<StartInfo> GetStartAsync();
}
