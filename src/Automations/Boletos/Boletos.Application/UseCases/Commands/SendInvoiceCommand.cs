using Boletos.Domain.Enuns;
using CRM.Application.Interfaces;

namespace Boletos.Application.UseCases.SedInvoices;

    public record SendInvoiceCommand : ICommand
{
    public InvoiceType Type { get; set; }

}

