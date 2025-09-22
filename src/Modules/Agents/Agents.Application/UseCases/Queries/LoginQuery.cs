using System.ComponentModel.DataAnnotations;
using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Queries;

public record LoginQuery(
    [EmailAddress] string Email,
    string Password) : IQuery<string>;