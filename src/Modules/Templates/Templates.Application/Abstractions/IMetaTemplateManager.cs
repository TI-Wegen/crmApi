using Templates.Domain.Aggregates;

namespace Templates.Application.Abstractions
{
    public interface IMetaTemplateManager
    {
        Task CriarTemplateNaMetaAsync(MessageTemplate template);

    }

}
