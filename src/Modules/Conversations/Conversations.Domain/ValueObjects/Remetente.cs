using Conversations.Domain.Enuns;
using Conversations.Domain.Exceptions;

namespace Conversations.Domain.ValueObjects;

    public class Remetente
{
    private RemetenteTipo cliente;
    private Guid guid;

    public RemetenteTipo Tipo { get; init; }
    public Guid? AgenteId { get; init; } // Nulo se o remetente for o Cliente
    public Remetente(RemetenteTipo tipo, Guid? agenteId = null)
    {
        if (tipo == RemetenteTipo.Agente && agenteId == null)
        {
            throw new DomainException("Um remetente do tipo Agente precisa de um AgenteId.");
        }
        Tipo = tipo;
        AgenteId = agenteId;
    }

 

    public static Remetente Cliente() => new(RemetenteTipo.Cliente);
    public static Remetente Agente(Guid agenteId) => new(RemetenteTipo.Agente, agenteId);
}

