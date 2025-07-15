using Agents.Application.Dtos;
using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries;

    public record GetSetoresQuery() : IQuery<IEnumerable<SetorDto>>;



