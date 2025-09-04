using CRM.Application.Dto;
using CRM.Application.Interfaces;
using Tags.Application.Dtos;

namespace Tags.Application.UseCases.Queries;

public record GetAllTagsQuery(int PageNumber = 1, int PageSize = 20) : IQuery<PaginationDto<TagDto>>;
