namespace CalendAgro.Services;

// Authentification locale : téléphone (+237) ou e-mail + mot de passe.
// Implémentation réelle (hachage PBKDF2) plus tard ; factice pour la phase B.
public sealed record AuthResult(bool Succeeded, string? Erreur = null);

public interface IAuthService
{
    Task<AuthResult> SignInAsync(string identifiant, string motDePasse);

    Task<AuthResult> SignUpAsync(string nom, string identifiant, string motDePasse, string telephone);

    Task DemanderCodeAsync(string identifiant);

    Task<AuthResult> RedefinirMotDePasseAsync(string code, string nouveauMotDePasse);

    bool IsIdentifiantValid(string identifiant);

    bool IsTelephoneValide(string telephone);

    bool IsNomValid(string nom);

    bool IsMotDePasseValid(string motDePasse);
}
