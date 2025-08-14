using Boletos.Application.UseCases.SedInvoices;
using Boletos.Domain.Entities;
using CRM.Application.Interfaces;

namespace Boletos.Application.Interrfaces;

public interface ISendInvoicesHandler : ICommandHandler<SendInvoiceCommand, IEnumerable<Client>>
{
    Task<IEnumerable<Client>> HandleAsync(SendInvoiceCommand input, CancellationToken cancellationToken = default);
}

