using CRM.Application.Interfaces;
using Tags.Application.Dtos;
using Tags.Application.Mappers;
using Tags.Domain.repository;
using Tags.Domain.Aggregates;

namespace Tags.Application.UseCases.Commands.Handler;

public class CriarTagHandler : ICommandHandler<CriarTagCommand, TagDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CriarTagHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TagDto> HandleAsync(CriarTagCommand command, CancellationToken cancellationToken = default)
    {
        var existingAgent = await _tagRepository.GetByNameAsync(command.Nome, cancellationToken);
        
        if (existingAgent is not null)
        {
            throw new Exception($"Já existe um agente com o nome '{command.Nome}'.");
        }

        var tags = Tags.Domain.Aggregates.Tags.Criar(command.Nome, command.Cor, command.Descricao);
        await _tagRepository.AddAsync(tags, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tags.ToDto();
    }
}
