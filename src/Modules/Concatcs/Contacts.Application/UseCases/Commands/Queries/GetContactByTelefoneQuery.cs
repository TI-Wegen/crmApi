using Contacts.Application.Dtos;
using CRM.Application.Interfaces;

namespace Contacts.Application.UseCases.Commands.Queries;

public record GetContactByTelefoneQuery(string Telefone) : IQuery<ContatoDto?>;


