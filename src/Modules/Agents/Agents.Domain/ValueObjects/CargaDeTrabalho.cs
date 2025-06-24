// Em Modules/Agents/Domain/ValueObjects/
using CRM.Domain.Exceptions;

// Usamos 'record' para imutabilidade e comparação baseada em valor.
public record CargaDeTrabalho
{
    public int Valor { get; }

    private CargaDeTrabalho(int valor)
    {
        if (valor < 0) throw new DomainException("A carga de trabalho não pode ser negativa.");
        Valor = valor;
    }

    public static CargaDeTrabalho Nenhuma() => new(0);

    public CargaDeTrabalho Incrementar() => new(Valor + 1);
    public CargaDeTrabalho Decrementar() => new(Valor - 1);
}