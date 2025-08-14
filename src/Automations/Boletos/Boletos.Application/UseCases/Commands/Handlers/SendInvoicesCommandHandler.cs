using Agents.Domain.Enuns;
using Agents.Domain.Repository;
using Boletos.Application.Interrfaces;
using Boletos.Application.UseCases.SedInvoices;
using Boletos.Domain.Entities;
using Boletos.Domain.Enuns;
using Boletos.Domain.Repositories;
using Contacts.Domain.Aggregates;
using Contacts.Domain.Repository;
using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
using CRM.Application.Interfaces;
using CRM.Application.ValueObject;
using CRM.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Runtime;

namespace Boletos.Application.UseCases.Commands.Handlers;

public class SendInvoicesCommandHandler : ISendInvoicesHandler
{
    private readonly IClientRepository _clientRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentServices _documentServices;
    private readonly IReportServices _reportServices;
    private readonly IContactRepository _contactRepositoy;
    private readonly IConversationRepository _conversationRepository;
    private readonly IAtendimentoRepository _atendimentoRepository;
    private readonly IMensageriaBotService _messageriaBotService;
    private readonly IAgentRepository _agentRepository;
    private readonly string _platformName;

    public SendInvoicesCommandHandler(IClientRepository clientRepository,
        IFileStorageService fileStorageService,
        IDocumentServices documentServices,
        IReportServices reportServices,
        IContactRepository contactRepositoy,
        IMensageriaBotService messageriaBotService,
        IConversationRepository conversationRepository,
        IAtendimentoRepository atendimentoRepository,
        IAgentRepository agentRepository,
        IConfiguration configuration
        )
    {
        _clientRepository = clientRepository;
        _fileStorageService = fileStorageService;
        _documentServices = documentServices;
        _reportServices = reportServices;
        _contactRepositoy = contactRepositoy;
        _messageriaBotService = messageriaBotService;
        _agentRepository = agentRepository;
        _conversationRepository = conversationRepository;
        _atendimentoRepository = atendimentoRepository;
        _platformName = configuration.GetValue<string>("PlatformName") ?? "PlatformName";
    }

    public async Task<IEnumerable<Client>> HandleAsync(SendInvoiceCommand input,
        CancellationToken cancellationToken = default)
    {
        var date = DateTime.Now;
        List<Client> clients = await GetClientsByType(input.Type);
        if (clients == null) return clients;

        Dictionary<string, List<string>> InvoiceSends = new();
        List<Client> clientsSend = new List<Client>();
        var agent = await _agentRepository
            .GetByNameAsync(SetorNome.Sistema.ToDbValue());

        foreach (Client client in clients)
        {
            if (client.ToSend)
            {
                var contact = await _contactRepositoy.GetByTelefoneAsync(client.Phone);
                if (contact == null)
                {
                    contact = Contato.Criar(client.Name, client.Phone, "");
                    await _contactRepositoy.AddAsync(contact);
                }
                var conversation = await _conversationRepository.FindActiveByContactIdAsync(contact.Id);
                if (conversation == null)
                {
                    conversation = Conversa.Iniciar(contact.Id, contact.Nome);
                    await _conversationRepository.AddAsync(conversation);
                }
                var atendimento = await _atendimentoRepository.FindActiveByConversaIdAsync(conversation.Id);
                if (atendimento == null)
                {
                    atendimento = Atendimento.IniciarPorAutomacao(conversation.Id, agent.Id);
                    await _atendimentoRepository.AddAsync(atendimento);
                }
              
                await ProcessDocumentGeneration(client, InvoiceSends);

                await ProcessMessageSending(client, input.Type, InvoiceSends, clientsSend, atendimento.Id);

            }

            await _clientRepository.CreateWpp(client);
        }

        return clientsSend;
    }

    private async Task<dynamic> GetClientsByType(InvoiceType type)
    {
        return type switch
        {
            InvoiceType.Generate => await _clientRepository.GetAllInvoicesGenerate(),
            InvoiceType.DueDate => await _clientRepository.GetAllInvoicesToDueDate(),
            InvoiceType.ThreeDays => await _clientRepository.GetAllInvoices3Days(),
            _ => throw new ArgumentException("Tipo de invoice inválido")
        };
    }

    private async Task ProcessDocumentGeneration(Client client, Dictionary<string, List<string>> InvoiceSends)
    {
        try
        {
            if (string.IsNullOrEmpty(client.DocumentUrl))
            {
                var fileName = $"{client.IdConta}_{client.TimeStamp}.pdf";
                //await _fileStorageService.DeleteFileAsync(fileName , BucketTypeEnum.Automations);


                client.CretaeUrlReportDowload(nomePlataforma: _platformName);
                var pdfBase64 = await _clientRepository.GetBoletoBase64ById(client.IdConta);

                var reportPath = await _reportServices.GetReport(client.EconomyUrl, client.ReportFileName);

                var mergeResult = await _documentServices.MergeDocument(pdfBase64, reportPath);

                var fileUrl = await _fileStorageService.UploadAsync(mergeResult, fileName, "application/pdf", BucketTypeEnum.Automations);

                client.UpdateDocumentUrl(fileUrl);
                await _reportServices.DeleteReport(reportPath);
            }
        }
        catch (Exception ex)
        {
            client.UpdateError($"Erro ao enviar fatura: {ex.Message}");
            if (!InvoiceSends.ContainsKey("Error"))
                InvoiceSends["Error"] = new List<string>();
            InvoiceSends["Error"].Add($"{client.IdConta} = {client.MessageStatus}");
        }
    }

    private async Task ProcessMessageSending(Client client, InvoiceType type, Dictionary<string, List<string>> InvoiceSends, List<Client> clientsSend, Guid atendimentoId)
    {
        var phone = client.Phone;
        var templateConfig = GetTemplateConfig(type, client);

        var inputLembrete = new SendTemplateInput(
            to: "5531984354960",
            templateName: templateConfig.TemplateName,
            parameters: templateConfig.Parameters,
            type: TemplateType.Document,
            documentUrl: client.DocumentUrl);

        var messageId = await _messageriaBotService.EnviarETemplateAsync(
            atendimentoId: atendimentoId,
            sendTemplateInput: inputLembrete
            );


        if (messageId is not null)
        {
            if (!InvoiceSends.ContainsKey(client.Motive))
                InvoiceSends[client.Motive] = new List<string>();
            InvoiceSends[client.Motive].Add($"{client.IdConta}");

            await UpdateClientByType(client, type);

            clientsSend.Add(client);
        }
        else
        {
            if (!InvoiceSends.ContainsKey("Error"))
                InvoiceSends["Error"] = new List<string>();

            InvoiceSends["Error"].Add($"{client.IdConta} = {messageId}");

            if (client.Phone.Length < 8)
            {
                client.UpdateError("conta sem número para envio");
            }
            else
            {
                client.UpdateError(messageId.ToString());
            }
        }

    }

    private (string TemplateName, List<string> Parameters) GetTemplateConfig(InvoiceType type, Client client)
    {
        return type switch
        {
            InvoiceType.Generate => ("lembrete_fatura", new List<string>
            {
                client.Name.Trim(),
                client.DaysToDueDate.ToString(),
                client.IdConta.ToString(),
                client.Reference,
                client.DueDate.ToString("dd/MM/yyyy"),
                client.Economy.ToString("N2")
            }),
            InvoiceType.DueDate => ("vencimento", new List<string>
            {
                client.Name,
                client.IdConta.ToString(),
                client.Reference,
                client.DueDate.ToString("dd/MM/yyyy"),
                client.Economy.ToString("N2")
            }),
            InvoiceType.ThreeDays => ("lembrete_fatura", new List<string>
            {
                client.Name,
                client.DaysToDueDate.ToString(),
                client.IdConta.ToString(),
                client.Reference,
                client.DueDate.ToString("dd/MM/yyyy"),
                client.Economy.ToString("N2")
            }),
            _ => throw new ArgumentException("Tipo de template inválido")
        };
    }

    private async Task UpdateClientByType(Client client, InvoiceType type)
    {
        switch (type)
        {
            case InvoiceType.Generate:
                await _clientRepository.UpdateGenerateAsync(client);
                break;
            case InvoiceType.DueDate:
                await _clientRepository.UpdateDueDateAsync(client);
                break;
            case InvoiceType.ThreeDays:
                await _clientRepository.Update3DaysAsync(client);
                break;
        }
    }
}

