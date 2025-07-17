using Conversations.Application.Dtos;
using CRM.Application.Interfaces;
using Metrics.Application.Dtos;

namespace Metrics.Application.UseCases.Queries;

    public class GetTemplatesSentPerAgentQuery(DateTime startDate, DateTime endDate) : IQuery<IEnumerable<TemplatesSentPerAgentDto>>
    {
        public DateTime StartDate { get; } = startDate;
        public DateTime EndDate { get; } = endDate;
}

