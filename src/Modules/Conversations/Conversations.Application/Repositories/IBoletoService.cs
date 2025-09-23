using Conversations.Application.Dtos;

namespace Conversations.Application.Repositories
{
    public interface IBoletoService
    {
        Task<BoletoDto?> GetBoletoAsync(int contaId);
        Task<IEnumerable<BoletoDto>> GetBoletosAbertosAsync(string cpf);
    }
}