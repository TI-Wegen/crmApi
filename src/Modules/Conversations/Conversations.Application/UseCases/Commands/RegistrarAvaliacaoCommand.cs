using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record RegistrarAvaliacaoCommand(Guid AtendimentoId, int Nota) : ICommand;
