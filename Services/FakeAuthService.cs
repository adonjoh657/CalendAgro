using System.Text.RegularExpressions;
using CalendAgro.Resources.Strings;

namespace CalendAgro.Services;

// Données factices : seul le compte de démonstration passe.
// Compte de démonstration — développements uniquement, jamais en production.
public sealed partial class FakeAuthService : IAuthService
{
    private const string DemoIdentifiant = "mireille@example.cm";
    private const string DemoMotDePasse = "haricot12";
    private const int NomLongueurMinimale = 2;
    private const int MotDePasseLongueurMinimale = 8;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^(\+237)?6\d{8}$")]
    private static partial Regex TelephoneRegex();

    public async Task<AuthResult> SignInAsync(string identifiant, string motDePasse)
    {
        await Task.Delay(600);
        var ok = string.Equals(identifiant.Trim(), DemoIdentifiant, StringComparison.OrdinalIgnoreCase)
            && motDePasse == DemoMotDePasse;
        return new AuthResult(ok);
    }

    public async Task<AuthResult> SignUpAsync(string nom, string identifiant, string motDePasse, string telephone)
    {
        await Task.Delay(600);
        if (string.Equals(identifiant.Trim(), DemoIdentifiant, StringComparison.OrdinalIgnoreCase))
        {
            return new AuthResult(false, AppResources.Inscription_CompteExistant);
        }
        return new AuthResult(true);
    }

    // Récupération factice : le code de démonstration est accepté.
    // À retirer avec l'implémentation réelle (question secrète locale).
    private const string DemoCode = "123456";

    public async Task DemanderCodeAsync(string identifiant)
    {
        await Task.Delay(600);
    }

    public async Task<AuthResult> RedefinirMotDePasseAsync(string code, string nouveauMotDePasse)
    {
        await Task.Delay(600);
        return code.Trim() == DemoCode
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
