namespace CalendAgro.Services;

// Une culture portant le même nom (même identifiant) existe déjà.
public sealed class CultureExistanteException : Exception
{
    public CultureExistanteException()
        : base("Une culture de même nom existe déjà.")
    {
    }
}
