using System.Text.RegularExpressions;
using CalendAgro.Resources.Strings;

namespace CalendAgro.Services;

// Authentification locale factice : aucune donnée n'est conservée et il
// n'existe aucun compte de démonstration. Elle sera remplacée par un compte
// local (hachage PBKDF2) puis par Supabase Auth (PLAN 28-31).
public sealed partial class FakeAuthService : IAuthService
{
    private const int NomLongueurMinimale = 2;
    private const int MotDePasseLongueurMinimale = 8;

    private readonly IProfilService _profil;

    public FakeAuthService(IProfilService profil) => _profil = profil;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^(\+237)?6\d{8}$")]
    private static partial Regex TelephoneRegex();

    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex CodeRegex();

    public async Task<AuthResult> SignInAsync(string identifiant, string motDePasse)
    {
        await Task.Delay(600);
        var valide = IsIdentifiantValid(identifiant) && IsMotDePasseValid(motDePasse);
        if (valide)
        {
            var identifiantNettoye = identifiant.Trim();
            var existant = await _profil.GetProfilAsync();
            var nom = existant is not null
                && string.Equals(existant.Identifiant, identifiantNettoye, StringComparison.OrdinalIgnoreCase)
                ? existant.Nom
                : identifiantNettoye;
            await _profil.EnregistrerAsync(nom, identifiantNettoye, existant?.Telephone ?? string.Empty);
        }

        return new AuthResult(valide);
    }

    public async Task<AuthResult> SignUpAsync(string nom, string identifiant, string motDePasse, string telephone)
    {
        await Task.Delay(600);
        await _profil.EnregistrerAsync(nom, identifiant, telephone);
        return new AuthResult(true);
    }

    public async Task DemanderCodeAsync(string identifiant)
    {
        await Task.Delay(600);
    }

    // Accepte tout code à 6 chiffres tant que l'envoi réel du code
    // (Supabase, lien par e-mail) n'est pas branché.
    public async Task<AuthResult> RedefinirMotDePasseAsync(string code, string nouveauMotDePasse)
    {
        await Task.Delay(600);
        return CodeRegex().IsMatch(code.Trim()) && IsMotDePasseValid(nouveauMotDePasse)
            ? new AuthResult(true)
            : new AuthResult(false, AppResources.Mdp_CodeErreur);
    }

    public bool IsIdentifiantValid(string identifiant)
    {
        var valeur = identifiant.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        return EmailRegex().IsMatch(valeur) || TelephoneRegex().IsMatch(valeur);
    }

    public bool IsTelephoneValide(string telephone)
    {
        var valeur = telephone.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        return TelephoneRegex().IsMatch(valeur);
    }

    public bool IsNomValid(string nom) => nom.Trim().Length >= NomLongueurMinimale;

    public bool IsMotDePasseValid(string motDePasse) => motDePasse.Length >= MotDePasseLongueurMinimale;
}
