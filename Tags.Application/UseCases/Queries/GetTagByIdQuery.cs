using CRM.Application.Interfaces;
using Tags.Application.Dtos;

namespace Tags.Application.UseCases.Queries;

public record GetTagByIdQuery(Guid Guid) : IQuery<TagDto>;