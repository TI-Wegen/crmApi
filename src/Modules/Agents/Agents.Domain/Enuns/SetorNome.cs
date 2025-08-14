namespace Agents.Domain.Enuns;
public enum SetorNome
{
    Financeiro,
    Comercial,
    Admin,
    Sistema
}

public static class SetorNomeExtensions
{
    public static string ToDbValue(this SetorNome setor)
    {
        return setor switch
        {
            SetorNome.Financeiro => "Financeiro",
            SetorNome.Comercial => "Comercial",
            SetorNome.Admin => "Administracao",
            SetorNome.Sistema => "Sistema",
            _ => throw new ArgumentOutOfRangeException(nameof(setor), setor, null)
        };
    }
}