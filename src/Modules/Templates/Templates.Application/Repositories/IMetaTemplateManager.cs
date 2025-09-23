using Templates.Domain.Entities;

namespace Templates.Application.Repositories
{
    public interface IMetaTemplateManager
    {
        Task CriarTemplateNaMetaAsync(MessageTemplate template);
    }
}