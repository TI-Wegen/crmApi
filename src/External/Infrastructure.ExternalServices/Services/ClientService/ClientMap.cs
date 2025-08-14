using Boletos.Domain.Entities;

namespace Infrastructure.ExternalServices.Services.ClientService;

    public class ClientMap
{
    public static Client MapToClient(ClientDto dto)
    {
        bool ths = !string.IsNullOrEmpty(dto.DemandaHFPInjetada) && dto.TipoInstalacao.ToUpper().Contains("THS");
        bool envio3Dias = dto.flagWpp3d.ToUpper() == "SIM" || dto.flagWpp3d.ToUpper() == "S";
        bool envioGeracao = dto.flagWppGeracao.ToUpper() == "SIM" || dto.flagWppGeracao.ToUpper() == "S";
        bool foreigner = dto.FlagWppEstrangeiro.ToUpper() == "SIM" || dto.FlagWppEstrangeiro.ToUpper() == "S";
        bool envioVencimento = dto.flagWppVencimento.ToUpper() == "SIM" || dto.flagWppVencimento.ToUpper() == "S";

        bool enviadoPelaMetaGeracao = dto.GeracaoMeta.ToUpper() == "SIM" || dto.GeracaoMeta.ToUpper() == "S";
        bool enviadoPelaMeta3Dias = dto.MetaEnvio3Dias.ToUpper() == "SIM" || dto.MetaEnvio3Dias.ToUpper() == "S";
        bool eviadoPelaMetaVencimento = dto.MEtaEnvioVencimento.ToUpper() == "SIM" || dto.MEtaEnvioVencimento.ToUpper() == "S";


        return new Client(
        name: dto.TitularConta,
        phone: dto.WhatsappRecebeConta,
        economy: dto.RateioEconomia,
        invoice: dto.RateioTotalRoi,
        dueDate: dto.DataVencimento,
        reference: dto.RateioReferenciaMes,
        ths: ths,
        idConta: dto.IdConta,
        timestamp: dto.Timestamp,
        send3Days: envio3Dias,
        generate: envioGeracao,
        sendDueDate: envioVencimento,
        documentUrl: null,
        foreigner: foreigner,
        send3DaysMeta: enviadoPelaMeta3Dias,
        sendGenerationMeta: enviadoPelaMetaGeracao,
        sendDueDateMeta: eviadoPelaMetaVencimento,
        discount: dto.percentualDescontoTarifaDistribuidora
       );
    }
}

