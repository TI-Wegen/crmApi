namespace Contacts.Application.UseCases.Commands.Queries;

// Em Modules/Contacts/Application/UseCases/Queries/
using Contacts.Application.Dtos;
using CRM.Application.Interfaces;

public record GetContactByIdQuery(Guid ContactId) : IQuery<ContatoDto>;