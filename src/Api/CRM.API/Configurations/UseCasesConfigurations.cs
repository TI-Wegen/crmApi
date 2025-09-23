using Agents.Application.Dtos;
using Agents.Application.Repositories;
using Agents.Application.UseCases.Commands;
using Agents.Application.UseCases.Commands.Handlers;
using Agents.Application.UseCases.Queries;
using Agents.Application.UseCases.Queries.Handler;
using Agents.Infrastructure.Adapters;
using Contacts.Application.Abstractions;
using Contacts.Application.Dtos;
using Contacts.Application.Repositories;
using Contacts.Application.UseCases.Commands;
using Contacts.Application.UseCases.Commands.Handlers;
using Contacts.Application.UseCases.Commands.Queries;
using Contacts.Application.UseCases.Commands.Queries.Handlers;
using Contacts.Application.UseCases.Queries.Handlers;
using Contacts.Infrastructure.Adapters;
using Conversations.Application.Dtos;
using Conversations.Application.Repositories;
using Conversations.Application.Services;
using Conversations.Application.UseCases.Commands;
using Conversations.Application.UseCases.Commands.Handlers;
using Conversations.Application.UseCases.Events;
using Conversations.Application.UseCases.Queries;
using Conversations.Application.UseCases.Queries.Handlers;
using Conversations.Infrastructure.Adapters;
using Conversations.Infrastructure.Jobs;
using Conversations.Infrastructure.Services;
using CRM.API.Services;
using CRM.Application.Dto;
using CRM.Application.Interfaces;
using CRM.Domain.DomainEvents;
using CRM.Infrastructure.Config.Meta;
using CRM.Infrastructure.Database.Configurations;
using CRM.Infrastructure.Services;
using Dashboard.Application.Dtos;
using Dashboard.Application.Repository;
using Dashboard.Application.UseCases.Queries;
using Dashboard.Application.UseCases.Queries.Handler;
using Dashboard.Infrastructure.Adapters;
using Infrastructure.ExternalServices.Services;
using Infrastructure.ExternalServices.Services.Meta;
using Tags.Application.Dtos;
using Tags.Application.repositories;
using Tags.Application.UseCases.Commands;
using Tags.Application.UseCases.Commands.Handler;
using Tags.Application.UseCases.Queries;
using Tags.Application.UseCases.Queries.Handler;
using Tags.Infrastructure.Adapters;
using Templates.Application.Dtos;
using Templates.Application.Repositories;
using Templates.Application.UseCases.Commands;
using Templates.Application.UseCases.Commands.Handler;
using Templates.Application.UseCases.Queries;
using Templates.Application.UseCases.Queries.Handler;
using Templates.Infrastructure.Adapters;
using Templates.Infrastructure.Services;
using static Conversations.Domain.Events.AtendimentoEvent;
using AddTagCommand = Conversations.Application.UseCases.Commands.AddTagCommand;
using AtualizarAgenteCommand = Agents.Application.UseCases.Commands.AtualizarAgenteCommand;

namespace CRM.API.Configurations;

public static class UseCaseConfigurations
{
    public static IServiceCollection AddUseCases(
        this IServiceCollection services)
    {
        services.AddRepositories();
        services.AddHandlers();
        services.AddServicesConfiguration();
        return services;
    }

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        services.AddScoped<IConversationRepository, ConversationAdapter>();
        services.AddScoped<IContactRepository, ContactAdapter>();
        services.AddScoped<IAgentRepository, AgentAdapter>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITemplateRepository, TemplateAdapter>();
        services.AddScoped<IAtendimentoRepository, AtendimentoAdapter>();
        services.AddScoped<ITagRepository, TagAdapter>();
        services.AddScoped<IMensagemRepository, MensagemAdapter>();
        services.AddScoped<IDashboardRepository, DashboardAdapter>();

        return services;
    }

    private static IServiceCollection AddServicesConfiguration(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IConversationReadService, DapperConversationReadService>();
        services.AddScoped<IMessageDeduplicationService, RedisMessageDeduplicationService>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IBoletoService, BoletoService>();
        services.AddScoped<IMetaTemplateManager, MetaTemplateManager>();
        services.AddScoped<IDomainEventHandler<ConversaResolvidaEvent>, ConversaResolvidaEventHandler>();
        services.AddScoped<IDomainEventHandler<AtendimentoResolvidoEvent>, AtendimentoResolvidoEventHandler>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDistributedLock, RedisDistributedLock>();
        services.AddScoped<IMessageBufferService, RedisMessageBufferService>();
        services.AddScoped<IMensageriaBotService, MensageriaBotService>();
        services.AddScoped<IMetaMediaService, MetaMediaService>();
        services.AddScoped<IMetaContactService, MetaContactService>();
        services.AddScoped<IBotSessionCache, RedisBotSessionCache>();
        services.AddScoped<IDistributedLock, RedisDistributedLock>();

        services.AddScoped<CleanExpiredBotSessionsJob>();
        return services;
    }

    private static IServiceCollection AddHandlers(
        this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<AtribuirAgenteCommand>, AtribuirAgenteCommandHandler>();
        services
            .AddScoped<IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto>,
                GetConversationByIdQueryHandler>();
        services.AddScoped<ICommandHandler<IniciarConversaCommand, Guid>, IniciarConversaCommandHandler>();
        services.AddScoped<ICommandHandler<AdicionarMensagemCommand, MessageDto>, AdicionarMensagemCommandHandler>();
        services.AddScoped<ICommandHandler<ResolverAtendimentoCommand>, ResolverAtendimentoCommandHandler>();
        services.AddScoped<ICommandHandler<TransferirAtendimentoCommand>, TransferirAtendimentoCommandHandler>();
        services
            .AddScoped<IQueryHandler<GetAllConversationsQuery, IEnumerable<ConversationSummaryDto>>,
                GetAllConversationsQueryHandler>();
        services.AddScoped<ICommandHandler<ProcessarRespostaDoMenuCommand>, ProcessarRespostaDoMenuCommandHandler>();
        services.AddScoped<ICommandHandler<RegistrarAvaliacaoCommand>, RegistrarAvaliacaoCommandHandler>();
        services.AddScoped<IQueryHandler<GetActiveChatQuery, ActiveChatDto>, GetActiveChatQueryHandler>();

        services.AddScoped<ICommandHandler<CriarAgenteCommand, AgenteDto>, CriarAgenteCommandHandler>();
        services.AddScoped<ICommandHandler<AtualizarAgenteCommand>, AtualizarAgenteCommandHandler>();
        services.AddScoped<IQueryHandler<GetAllAgentsQuery, IEnumerable<AgenteDto>>, GetAllAgentsQueryHandler>();
        services.AddScoped<IQueryHandler<GetAgentByIdQuery, AgenteDto>, GetAgentByIdQueryHandler>();
        services.AddScoped<ICommandHandler<InativarAgenteCommand>, InativarAgenteCommandHandler>();
        services.AddScoped<IQueryHandler<LoginQuery, string>, LoginQueryHandler>();
        services.AddScoped<IQueryHandler<GetSetoresQuery, IEnumerable<SetorDto>>, GetSetoresQueryHandler>();
        services
            .AddScoped<IQueryHandler<GetConversationByContactQuery, ConversationDetailsDto>,
                GetConversationByContactQueryHandler>();

        services.AddScoped<ICommandHandler<CriarContatoCommand, ContatoDto>, CriarContatoCommandHandler>();
        services.AddScoped<IQueryHandler<GetContactByIdQuery, ContatoDto>, GetContactByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllContactsQuery, IEnumerable<ContatoDto>>, GetAllContactsQueryHandler>();
        services.AddScoped<ICommandHandler<AtualizarContatoCommand>, AtualizarContatoCommandHandler>();
        services.AddScoped<ICommandHandler<InativarContatoCommand>, InativarContatoCommandHandler>();
        services.AddScoped<IQueryHandler<GetContactByTelefoneQuery, ContatoDto?>, GetContactByTelefoneQueryHandler>();
        services.AddScoped<ICommandHandler<EnviarTemplateCommand>, EnviarTemplateCommandHandler>();
        services.AddScoped<ICommandHandler<RegistrarMensagemEnviadaCommand>, RegistrarMensagemEnviadaCommandHandler>();
        services.AddScoped<ICommandHandler<AtualizarAvatarContatoCommand>, AtualizarAvatarContatoCommandHandler>();
        services.AddScoped<ICommandHandler<AddTagCommand>, AddTagCommandHandler>();

        services.AddScoped<ICommandHandler<CriarTemplateCommand, TemplateDto>, CriarTemplateCommandHandler>();
        services
            .AddScoped<IQueryHandler<GetAllTemplatesQuery, IEnumerable<TemplateDto>>, GetAllTemplatesQueryHandler>();
        services.AddScoped<ICommandHandler<AtualizarStatusTemplateCommand>, AtualizarStatusTemplateCommandHandler>();
        services.AddScoped<ICommandHandler<AdicionaReacaoMensagemCommand>, AdicionaReacaoMensagemCommandHandler>();

        services.AddScoped<ICommandHandler<CriarTagCommand, TagDto>, CriarTagHandler>();
        services.AddScoped<ICommandHandler<AtualizarTagCommand, TagDto>, AtualizarTagHandler>();
        services.AddScoped<ICommandHandler<InativarTagCommand, TagDto>, InativarTagHandler>();
        
        services.AddScoped<IQueryHandler<DashboardFullQuery, DashboardFullResponseQuery>, DashboardFullQueryHandler>();
        services.AddScoped<IQueryHandler<DashboardPersonalQuery, DashboardPersonalResponseQuery>, DashboardPersonalQueryHandler>();

        services.AddScoped<IQueryHandler<GetAllTagsQuery, PaginationDto<TagDto>>, GetAllTagsQueryHandler>();
        services.AddScoped<IQueryHandler<GetTagByIdQuery, TagDto>, GetTagsByIdQueryHandler>();

        return services;
    }
}