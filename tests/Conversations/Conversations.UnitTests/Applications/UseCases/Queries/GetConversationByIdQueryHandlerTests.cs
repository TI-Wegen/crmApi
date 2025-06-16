using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Exceptions;
using Conversations.Application.UseCases.Queries;
using Conversations.Application.UseCases.Queries.Handlers;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Entities;
using Conversations.Domain.Enuns;
using Conversations.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Conversations.UnitTests.Applications.UseCases.Queries;

public class GetConversationByIdQueryHandlerTests
{
    private readonly Mock<IConversationRepository> _mockConversationRepository;
    private readonly GetConversationByIdQueryHandler _handler;

    public GetConversationByIdQueryHandlerTests()
    {
        _mockConversationRepository = new Mock<IConversationRepository>();
        _handler = new GetConversationByIdQueryHandler(_mockConversationRepository.Object);
    }

    // --- Cenário de Sucesso ---

    [Fact]
    public async Task HandleAsync_QuandoConversaExiste_DeveRetornarConversationDetailsDto()
    {
        // Arrange
        var conversaId = Guid.NewGuid();
        var query = new GetConversationByIdQuery(conversaId);

        // Criamos uma entidade de domínio 'Conversa' falsa para o repositório retornar.
        var conversaDeDominio = Conversa.Iniciar(Guid.NewGuid(), new Mensagem("Olá", new Remetente(RemetenteTipo.Cliente, Guid.NewGuid())));

        // Configuramos o mock para retornar nossa conversa falsa quando o método for chamado com o ID correto.
        _mockConversationRepository
            .Setup(repo => repo.GetByIdWithMessagesAsync(conversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversaDeDominio);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        // Verificamos se o resultado não é nulo e é do tipo esperado.
        result.Should().NotBeNull();
        result.Should().BeOfType<ConversationDetailsDto>();

        // Verificamos se o mapeamento foi correto (o ID do DTO deve ser o mesmo da entidade).
        result.Id.Should().Be(conversaDeDominio.Id);

        // Garantimos que o repositório foi chamado exatamente uma vez.
        _mockConversationRepository.Verify(repo => repo.GetByIdWithMessagesAsync(conversaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Cenário de Falha ---

    [Fact]
    public async Task HandleAsync_QuandoConversaNaoExiste_DeveLancarNotFoundException()
    {
        // Arrange
        var conversaId = Guid.NewGuid();
        var query = new GetConversationByIdQuery(conversaId);

        // Configuramos o mock para retornar 'null', simulando que a conversa não foi encontrada.
        _mockConversationRepository
            .Setup(repo => repo.GetByIdWithMessagesAsync(conversaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversa)null);

        // Act
        // Executamos a ação e guardamos a task para verificar a exceção.
        Func<Task> action = async () => await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        // Verificamos se a exceção do tipo 'NotFoundException' foi lançada.
        await action.Should().ThrowAsync<NotFoundException>()
              .WithMessage($"Conversa com o Id '{conversaId}' não encontrada.");
    }
}