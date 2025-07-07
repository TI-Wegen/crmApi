namespace Conversations.Domain.Enuns;

public enum SetorNome
{
    Financeiro,
    Comercial
}

public static class SetorNomeExtensions
{
    public static string ToDbValue(this SetorNome setor)
    {
        return setor switch
        {
            SetorNome.Financeiro => "Financeiro",
            SetorNome.Comercial => "Comercial",
            _ => throw new ArgumentOutOfRangeException(nameof(setor), setor, null)
        };
    }
}