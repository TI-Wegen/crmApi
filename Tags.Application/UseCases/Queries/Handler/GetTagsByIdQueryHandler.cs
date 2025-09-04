using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using Tags.Application.Dtos;
using Tags.Application.Mappers;
using Tags.Domain.repository;

namespace Tags.Application.UseCases.Queries.Handler;

public class GetTagsByIdQueryHandler : IQueryHandler<GetTagByIdQuery, TagDto>
{
    private readonly ITagRepository _tagsRepository;
    
    public GetTagsByIdQueryHandler(ITagRepository tagsRepository)
    {
        _tagsRepository = tagsRepository;
    }
    public async Task<TagDto> HandleAsync(GetTagByIdQuery query, CancellationToken cancellationToken = default)
    {
        var tags = await _tagsRepository.GetByIdAsync(query.Guid, cancellationToken);
        if (tags is null)
        {
            throw new NotFoundException($"Tag com o Id '{query.Guid}' não encontrado.");
            
        }
        
        return tags.ToDto();
    }
}