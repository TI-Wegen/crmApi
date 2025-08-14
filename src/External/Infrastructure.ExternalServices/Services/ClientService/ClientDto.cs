namespace Infrastructure.ExternalServices.Services.ClientService;

public class ClientDto
{
    public string TitularConta { get; set; }
    public string WhatsappRecebeConta { get; set; }
    public string RateioReferenciaMes { get; set; }
    public string RateioReferenciaAno { get; set; }
    public DateTime DataVencimento { get; set; }
    public int IdConta { get; set; }
    public decimal RateioTotalRoi { get; set; }
    public decimal RateioEconomia { get; set; }
    public string Timestamp { get; set; }
    public string GeracaoMeta { get; set; }
    public string MetaEnvio3Dias { get; set; }
    public string MEtaEnvioVencimento { get; set; }
    public string FlagWppEstrangeiro { get; set; }
    public string DemandaHFPInjetada { get; set; }
    public string TipoInstalacao { get; set; }
    public string flagWppGeracao { get; set; }
    public string flagWpp3d { get; set; }
    public string flagWppInadimplente { get; set; }
    public string flagWppVencimento { get; set; }
    public decimal percentualDescontoTarifaDistribuidora { get; set; }
}
