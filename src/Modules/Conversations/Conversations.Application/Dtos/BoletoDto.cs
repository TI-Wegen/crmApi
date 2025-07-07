namespace Conversations.Application.Dtos;

public record BoletoDto
{
    public int IdConta { get; init; }
    public int IdFatura { get; init; }
    public DateTime DataVencimento { get; init; }
    public string Referente { get; init; }
    public string PdfBoleto { get; init; }
    public string StatusBoleto { get; init; }
    public string Numinstalacao { get; init; }
}

