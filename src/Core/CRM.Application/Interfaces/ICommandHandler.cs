namespace CRM.Application.Interfaces;

public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
