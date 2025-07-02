namespace Agents.Application.UseCases.Commands.Handlers;

using Agents.Application.Dtos;
using Agents.Application.Mappers; 
using Agents.Domain.Aggregates;
using Agents.Domain.Repository;

using CRM.Application.Interfaces;

public class CriarAgenteCommandHandler : ICommandHandler<CriarAgenteCommand, AgenteDto>
{
    private readonly IAgentRepository _agentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarAgenteCommandHandler(IAgentRepository agentRepository, IUnitOfWork unitOfWork)
    {
        _agentRepository = agentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AgenteDto> HandleAsync(CriarAgenteCommand command, CancellationToken cancellationToken)
    {
        // 1. Validação da Aplicação: verificar se o e-mail já existe
        var existingAgent = await _agentRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingAgent is not null)
        {
            // Poderíamos criar uma exceção customizada da aplicação aqui
            throw new Exception($"Já existe um agente com o e-mail '{command.Email}'.");
        }

        // 2. Usar o método de fábrica do domínio para criar o agregado
        var agente = Agente.Criar(command.Nome, command.Email);
        agente.DefinirSenha(command.Senha);
        // 3. Adicionar ao repositório
        await _agentRepository.AddAsync(agente, cancellationToken);

        // 4. Salvar as alterações
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Mapear para DTO e retornar
        return agente.ToDto();
    }
}