using Boletos.Application.Interrfaces;
using Boletos.Application.UseCases.SedInvoices;
using Boletos.Domain.Enuns;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Jobs.Automations;

public class SendInvoicesJobs : IJobs
{
    private readonly ILogger<SendInvoicesJobs> _logger;
    private readonly ISendInvoicesHandler _sendInvoicesUseCase;

    public SendInvoicesJobs(ILogger<SendInvoicesJobs> logger,
        ISendInvoicesHandler sendInvoicesUseCase)
    {
        _logger = logger;
        _sendInvoicesUseCase = sendInvoicesUseCase;
    }

    public async Task Execute()
    {
        //await _sendInvoicesUseCase.HandleAsync(new SendInvoiceCommand
        //{
        //    Type = InvoiceType.Generate
        //});

        //await _sendInvoicesUseCase.HandleAsync(new SendInvoiceCommand
        //{
        //    Type = InvoiceType.DueDate
        //});

        //await _sendInvoicesUseCase.HandleAsync(new SendInvoiceCommand
        //{
        //    Type = InvoiceType.ThreeDays
        //});

    }
}

