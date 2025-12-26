namespace VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos
{
    public class DocumentCheckDto
    {
        public bool IsDocumentExist { get; set; }
        public int DocumentProcessCount { get; set; }
        public string DocumentNo { get; set; }

        public List<DocumentProcessList> DocumentProcessList { get; set; } = new();
    }

    public class DocumentProcessList
    {
        public string OrderNo { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}