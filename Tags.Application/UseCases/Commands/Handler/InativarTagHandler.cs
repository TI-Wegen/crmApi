using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using Tags.Application.Dtos;
using Tags.Application.Mappers;
using Tags.Application.repositories;

namespace Tags.Application.UseCases.Commands.Handler;

public class InativarTagHandler : ICommandHandler<InativarTagCommand, TagDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public InativarTagHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<TagDto> HandleAsync(InativarTagCommand command, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(command.Guid, cancellationToken);
        if (tag is null)
            throw new NotFoundException($"Tag com o Id '{command.Guid}' não encontrado.");

        tag.Inativar();

        await _tagRepository.UpdateAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return tag.ToDto();
    }
}
