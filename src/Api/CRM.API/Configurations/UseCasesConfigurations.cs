using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.UseCases.Commands;
using Conversations.Application.UseCases.Commands.Handlers;
using Conversations.Application.UseCases.Queries;
using Conversations.Application.UseCases.Queries.Handlers;
using Conversations.Infrastructure.Repositories;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();


        return services;
    }

    private static IServiceCollection AddHandlers(
    this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<AtribuirAgenteCommand>, AtribuirAgenteCommandHandler>();
        services.AddScoped<IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto>, GetConversationByIdQueryHandler>();
        services.AddScoped<ICommandHandler<IniciarConversaCommand, Guid>, IniciarConversaCommandHandler>();
        services.AddScoped<ICommandHandler<AdicionarMensagemCommand, MessageDto>, AdicionarMensagemCommandHandler>();


        return services;
    }
}