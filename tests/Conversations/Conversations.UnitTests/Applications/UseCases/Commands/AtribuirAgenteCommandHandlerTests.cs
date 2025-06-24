using Conversations.Application.Abstractions;
using Conversations.Application.UseCases.Commands;
using Conversations.Application.UseCases.Commands.Handlers;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using CRM.Domain.Exceptions;
using Conversations.Domain.ValueObjects;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace Conversations.UnitTests.Applications.UseCases.Commands;

public class AtribuirAgenteCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _mockConversationRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly AtribuirAgenteCommandHandler _handler;

    public AtribuirAgenteCommandHandlerTests()
    {
        _mockConversationRepository = new Mock<IConversationRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new AtribuirAgenteCommandHandler(_mockConversationRepository.Object, _mockUnitOfWork.Object);
    }

    // --- 1. Cenário de Sucesso ---

    [Fact]
    public async Task HandleAsync_QuandoConversaExisteEValida_DeveChamarAtribuirAgenteESalvar()
    {
        // Arrange
        var command = new AtribuirAgenteCommand(Guid.NewGuid(), Guid.NewGuid());

        // Criamos uma instância real do agregado em um estado válido para a operação.
        var conversa = Conversa.Iniciar(Guid.NewGuid(), new Mensagem("Teste", new Remetente( RemetenteTipo.Cliente, Guid.NewGuid())));

        _mockConversationRepository
            .Setup(repo => repo.GetByIdAsync(command.ConversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversa);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        // Verificamos se o estado do agregado foi alterado como esperado.
        conversa.AgenteId.Should().Be(command.AgenteId);
        conversa.Status.Should().Be(ConversationStatus.EmAtendimento);

        // Verificamos se os métodos de persistência foram chamados.
        _mockConversationRepository.Verify(repo => repo.UpdateAsync(conversa, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- 2. Cenário de Falha: Conversa Não Encontrada ---

    [Fact]
    public async Task HandleAsync_QuandoConversaNaoExiste_DeveLancarNotFoundException()
    {
        // Arrange
        var command = new AtribuirAgenteCommand(Guid.NewGuid(), Guid.NewGuid());

        _mockConversationRepository
            .Setup(repo => repo.GetByIdAsync(command.ConversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversa)null); // Simulando que o repositório não encontrou nada.

        // Act
        Func<Task> action = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();

        // É CRUCIAL garantir que, em caso de falha, nada seja salvo.
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- 3. Cenário de Falha: Regra de Domínio Violada ---

    [Fact]
    public async Task HandleAsync_QuandoRegraDeDominioEViolada_DeveLancarDomainExceptionESalvamentoNaoOcorre()
    {
        // Arrange
        var command = new AtribuirAgenteCommand(Guid.NewGuid(), Guid.NewGuid());

        // Criamos uma conversa em um estado que fará 'AtribuirAgente' falhar (ex: já resolvida).
        var conversaResolvida = Conversa.Iniciar(Guid.NewGuid(), new Mensagem("Teste", new Remetente(RemetenteTipo.Cliente, Guid.NewGuid())));
        conversaResolvida.AtribuirAgente(Guid.NewGuid());
        conversaResolvida.Resolver(); // Agora a conversa está no estado 'Resolvida'.

        _mockConversationRepository
            .Setup(repo => repo.GetByIdAsync(command.ConversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversaResolvida);

        // Act
        Func<Task> action = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        // Verificamos se a exceção vinda do domínio foi propagada.
        await action.Should().ThrowAsync<DomainException>();

        // Novamente, garantimos que a unidade de trabalho não tentou salvar um estado inválido.
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}