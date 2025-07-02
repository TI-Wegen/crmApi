using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries;

public record LoginQuery(string Email, string Password) : IQuery<string>; 