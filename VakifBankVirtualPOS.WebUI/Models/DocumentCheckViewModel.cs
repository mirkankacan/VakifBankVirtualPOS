namespace VakifBankVirtualPOS.WebUI.Models
{
    public class DocumentCheckViewModel
    {
        public bool IsDocumentExist { get; set; }
        public int DocumentProcessCount { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public List<DocumentProcessViewModel> DocumentProcessList { get; set; } = new();
    }

    public class DocumentProcessViewModel
    {
        public string OrderNo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}

