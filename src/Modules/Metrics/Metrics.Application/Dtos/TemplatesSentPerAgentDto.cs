namespace Metrics.Application.Dtos;

    public class TemplatesSentPerAgentDto
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; }
    public int TotalEnviado { get; set; }
    public DateTime LastSentAt { get; set; }
    public TemplatesSentPerAgentDto(Guid agentId, string agentName, int totalEnviado, DateTime lastSentAt)
    {
        AgentId = agentId;
        AgentName = agentName;
        TotalEnviado = totalEnviado;
        LastSentAt = lastSentAt;
    }
}

