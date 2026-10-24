namespace CalendAgro.Services;

// Session locale : compte existant ou usage sans compte.
// Implémentation réelle (Supabase Auth) plus tard ; factice pour la phase B.
public sealed record StartInfo(bool HasLocalAccount);

public interface ISessionService
{
    Task<StartInfo> GetStartAsync();
}
