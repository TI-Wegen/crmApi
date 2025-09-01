using Conversations.Application.Dtos;

namespace Conversations.Application.Abstractions
{
    public interface IBoletoService
    {
        Task<BoletoDto?> GetBoletoAsync(int IdConta);
        Task<IEnumerable<BoletoDto>> GetBoletosAbertosAsync(string cpf);
    }
}