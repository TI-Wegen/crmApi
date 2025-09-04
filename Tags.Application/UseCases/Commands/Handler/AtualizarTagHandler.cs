using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using Tags.Application.Dtos;
using Tags.Application.Mappers;
using Tags.Domain.repository;

namespace Tags.Application.UseCases.Commands.Handler;

public class AtualizarTagHandler : ICommandHandler<AtualizarTagCommand, TagDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarTagHandler(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TagDto> HandleAsync(AtualizarTagCommand command, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tag is null)
            throw new NotFoundException($"Tag com o Id '{command.Id}' não encontrado.");

        tag.Atualizar(command.Nome, command.Cor, command.Descricao);

        await _tagRepository.UpdateAsync(tag, cancellationToken); 
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToDto();
    }
}