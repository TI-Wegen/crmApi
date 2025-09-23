using Conversations.Application.Dtos;

namespace Conversations.Application.Repository
{
    public interface IBoletoService
    {
        Task<BoletoDto?> GetBoletoAsync(int contaId);
        Task<IEnumerable<BoletoDto>> GetBoletosAbertosAsync(string cpf);
    }
}