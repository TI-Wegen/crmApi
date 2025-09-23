using CRM.Application.Dto;
using CRM.Application.Interfaces;
using Tags.Application.Dtos;
using Tags.Application.repository;

namespace Tags.Application.UseCases.Queries.Handler;

public class GetAllTagsQueryHandler : IQueryHandler<GetAllTagsQuery, PaginationDto<TagDto>>
{
    private readonly ITagRepository _tagsRepository;

    public GetAllTagsQueryHandler(ITagRepository tagsRepository)
    {
        _tagsRepository = tagsRepository;
    }

    public async Task<PaginationDto<TagDto>> HandleAsync(GetAllTagsQuery query,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _tagsRepository.GetAllAsync(query.PageNumber, query.PageSize, cancellationToken);
        
        var tagDtos = pagedResult.Items.Select(x => new TagDto(
            x.Id,
            x.Nome,
            x.Cor,
            x.Descricao)).ToList();
        
        return PaginationDto<TagDto>.Create(
            tagDtos,
            query.PageNumber,
            query.PageSize,
            pagedResult.TotalCount
        );
    }
}
