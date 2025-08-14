using Boletos.Domain.Enuns;

namespace Boletos.Domain.Entities;

public class Client
{
    public Client(string name,
        string phone,
        decimal economy,
        decimal invoice,
        DateTime dueDate,
        string reference,
        bool generate,
        bool ths,
        int idConta,
        string timestamp,
        bool send3Days,
        bool sendDueDate,
        bool foreigner,
        bool sendGenerationMeta,
        bool send3DaysMeta,
        bool sendDueDateMeta,
        decimal discount,
        string? documentUrl = null)
    {
        Name = name;
        Invoice = invoice;
        DueDate = dueDate;
        Reference = reference;
        DocumentUrl = documentUrl;
        Generate = generate;
        Send3Days = send3Days;
        SendDueDate = sendDueDate;
        Ths = ths;
        TimeStamp = timestamp;
        IdConta = idConta;
        Foreigner = foreigner;
        MetaSendGeneration = sendGenerationMeta;
        MetaSend3Days = send3DaysMeta;
        MetaSendDueDate = sendDueDateMeta;
        Discount = discount;

        Phone = FormatToMetaPhone(phone, foreigner);
        DaysToDueDate = CalculateDaysToDueDate();
        Economy = CalculateEconomy(economy);
        ToSend = ToSendValidation();
    }

    public int IdConta { get; private set; }
    public string Name { get; private set; }
    public string Document { get; private set; }
    public string Phone { get; private set; }
    public decimal Economy { get; private set; }
    public decimal Invoice { get; private set; }
    public DateTime DueDate { get; private set; }
    public string Reference { get; private set; }
    public int DaysToDueDate { get; private set; }
    public decimal Discount { get; private set; }
    public string PdfBase64 { get; private set; }
    public string? DocumentUrl { get; private set; }
    public string EconomyUrl { get; private set; }
    public bool Generate { get; private set; }
    public bool Send3Days { get; private set; }
    public bool SendDueDate { get; private set; }

    public bool Foreigner { get; private set; }
    public bool MetaSendGeneration { get; private set; }
    public bool MetaSend3Days { get; private set; }
    public bool MetaSendDueDate { get; private set; }

    public string DueDateIn3days { get; private set; }
    public string TimeStamp { get; private set; }
    public string ReportFileName { get; private set; }
    public bool Ths { get; private set; }
    public InvoicesEnun Status { get; private set; }
    public bool ToSend { get; private set; }
    public string? MessageId { get; private set; }
    public string? MessageStatus { get; private set; }
    public string Motive { get; private set; }
    private int CalculateDaysToDueDate()
    {
        // Calcula os dias restantes para o vencimento
        var today = DateTime.Today;
        var daysToDueDate = (DueDate.Date - today).Days;

        if (daysToDueDate == 3)
            Status = InvoicesEnun.DueDate3;
        else if (daysToDueDate > 3)
            Status = InvoicesEnun.Generate;
        else if (daysToDueDate == 0)
            Status = InvoicesEnun.DueDate0;
        else
            Status = InvoicesEnun.Pending;

        return daysToDueDate;
    }

    public void CretaeUrlReportDowload(string nomePlataforma)
    {
        string url = @$"https://plataforma.{nomePlataforma}.com.br/FrmRelatorioEcoSobDemanda?idconta={IdConta}&ths={Ths}&impressao=sim&percentual=nao&ts={TimeStamp}";
        ReportFileName = $"relatorioPDF{TimeStamp}.pdf";
        EconomyUrl = url;
    }
    public void UpdateDocumentUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL não pode ser nula ou vazia.", nameof(url));
        DocumentUrl = url;
    }
    public string FormatToMetaPhone(string phone, bool foreigner)
    {
        ToSend = true;
        if (string.IsNullOrEmpty(phone)) ToSend = false;
        //throw new ArgumentException("Telefone não pode ser nulo ou vazio.", nameof(phone));

        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (foreigner) return digits.StartsWith("+") ? digits : "+" + digits;

        if (!phone.StartsWith("55")) digits = "55" + digits;

        if (digits.Length == 12) digits = digits.Insert(5, "9"); // Insere o 9 após DDI+DDD
        return "+" + digits;
    }

    public bool ToSendValidation()
    {

        if (!Generate && Status == InvoicesEnun.Generate)
        {
            MessageStatus = "O usuário optou por não receber envio ao gerar a conta";
            Motive = "EnvioComImpedimento";
            return false;
        }

        if (!Send3Days && Status == InvoicesEnun.DueDate3)
        {
            MessageStatus = "O usuário optou por não receber envio de 3 dias antes do vencimento";
            Motive = "EnvioComImpedimento";
            return false;
        }

        if (!SendDueDate && Status == InvoicesEnun.DueDate0)
        {
            MessageStatus = "O usuário optou por não receber envio no dia do vencimento";
            Motive = "EnvioComImpedimento";
            return false;
        }


        // envia se a data de vencimento estiver em 3 dias e se a flag for 'N',ou seja, não foi enviado
        if (Status == InvoicesEnun.DueDate3 && Send3Days && !MetaSend3Days)
        {
            Motive = "TresDias";
            return true;
        }
        // envia se a data de vencimento estiver acima de 3 dias e se a flag for 'N',ou seja, não foi enviado
        if (Status == InvoicesEnun.Generate && Generate && !MetaSendGeneration)
        {
            Motive = "Geracao";
            return true;
        }
        // envia se a data de vencimento estiver igual a 0  e se a flag for 'N',ou seja, não foi enviado
        if (Status == InvoicesEnun.DueDate0 && SendDueDate && !MetaSendDueDate)
        {
            Motive = "Vencimento";
            return true;
        }

        return false;
    }

    public decimal CalculateEconomy(decimal economy)
    {
        decimal discount = Discount * Invoice / (100 - Discount);
        if (discount <= 0) return Math.Round(economy, 2);

        return Math.Round(discount, 2);
    }

    public void UpdateError(string messageId)
    {
        MessageStatus = messageId;
        Motive = "EnvioComErro";
    }
}


