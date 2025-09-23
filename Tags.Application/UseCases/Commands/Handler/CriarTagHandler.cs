using CRM.Application.Interfaces;
using Tags.Application.Dtos;
using Tags.Application.Mappers;
using Tags.Application.repository;

namespace Tags.Application.UseCases.Commands.Handler;

public class CriarTagHandler : ICommandHandler<CriarTagCommand, TagDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CriarTagHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<TagDto> HandleAsync(CriarTagCommand command, CancellationToken cancellationToken = default)
    {
        var existingAgent = await _tagRepository.GetByNameAsync(command.Nome, cancellationToken);

        if (existingAgent is not null)
        {
            throw new Exception($"Já existe um agente com o nome '{command.Nome}'.");
        }

        var getCurrentUser = _userContext.GetCurrentUserId();

        if (getCurrentUser is null)
        {
            throw new ApplicationException("Usuário não logado");
        }

        var tags = Domain.Entities.Tags.Criar(command.Nome, command.Cor, command.Descricao ?? "",
            (Guid)getCurrentUser);
        await _tagRepository.AddAsync(tags, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tags.ToDto();
    }
}