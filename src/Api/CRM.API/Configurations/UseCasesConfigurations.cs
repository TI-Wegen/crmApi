using Agents.Application.Dtos;
using Agents.Application.UseCases.Commands;
using Agents.Application.UseCases.Commands.Handlers;
using Agents.Application.UseCases.Queries;
using Agents.Domain.Repository;
using Agents.Infrastructure.Repositories;
using Contacts.Application.Dtos;
using Contacts.Application.UseCases.Commands;
using Contacts.Application.UseCases.Commands.Handlers;
using Contacts.Application.UseCases.Commands.Queries;
using Contacts.Domain.Repository;
using Contacts.Infrastructure.Repositories;
using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Jobs;
using Conversations.Application.UseCases.Commands;
using Conversations.Application.UseCases.Commands.Handlers;
using Conversations.Application.UseCases.Queries;
using Conversations.Application.UseCases.Queries.Handlers;
using Conversations.Infrastructure.Repositories;
using Conversations.Infrastructure.Services;
using CRM.API.Services;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Database.Configurations;
using System.Net;

namespace CRM.API.Configurations;

public static class UseCaseConfigurations
{

    public static IServiceCollection AddUseCases(
        this IServiceCollection services)
    {

        services.AddRepositories();
        services.AddHandlers();
        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {

        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<IConversationReadService, DapperConversationReadService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ExpirarSessoesJob>();

        return services;
    }

    private static IServiceCollection AddHandlers(
    this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<AtribuirAgenteCommand>, AtribuirAgenteCommandHandler>();
        services.AddScoped<IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto>, GetConversationByIdQueryHandler>();
        services.AddScoped<ICommandHandler<IniciarConversaCommand, Guid>, IniciarConversaCommandHandler>();
        services.AddScoped<ICommandHandler<AdicionarMensagemCommand, MessageDto>, AdicionarMensagemCommandHandler>();
        services.AddScoped<ICommandHandler<ResolverConversaCommand>, ResolverConversaCommandHandler>();
        services.AddScoped<ICommandHandler<TransferirConversaCommand>, TransferirConversaCommandHandler>();
        services.AddScoped<ICommandHandler<ReabrirConversaCommand>, ReabrirConversaCommandHandler>();
        services.AddScoped<IQueryHandler<GetAllConversationsQuery, IEnumerable<ConversationSummaryDto>>, GetAllConversationsQueryHandler>();

        services.AddScoped<ICommandHandler<CriarAgenteCommand, AgenteDto>, CriarAgenteCommandHandler>();
        services.AddScoped<ICommandHandler<AtualizarAgenteCommand>, AtualizarAgenteCommandHandler>();
        services.AddScoped<IQueryHandler<GetAllAgentsQuery, IEnumerable<AgenteDto>>, GetAllAgentsQueryHandler>();
        services.AddScoped<IQueryHandler<GetAgentByIdQuery, AgenteDto>, GetAgentByIdQueryHandler>();
        services.AddScoped<ICommandHandler<InativarAgenteCommand>, InativarAgenteCommandHandler>();



        services.AddScoped<ICommandHandler<CriarContatoCommand, ContatoDto>, CriarContatoCommandHandler>();
        services.AddScoped<IQueryHandler<GetContactByIdQuery, ContatoDto>, GetContactByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllContactsQuery, IEnumerable<ContatoDto>>, GetAllContactsQueryHandler>();
        services.AddScoped<ICommandHandler<AtualizarContatoCommand>, AtualizarContatoCommandHandler>();
        services.AddScoped<ICommandHandler<InativarContatoCommand>, InativarContatoCommandHandler>();

      
        return services;
    }
}