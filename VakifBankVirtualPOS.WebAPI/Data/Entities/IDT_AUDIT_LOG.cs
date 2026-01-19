namespace VakifBankVirtualPOS.WebAPI.Data.Entities
{
    public class IDT_AUDIT_LOG
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ClientCode { get; set; }
        public string Operation { get; set; }
        public string TableName { get; set; }
        public string? OldValue { get; set; }
        public string NewValue { get; set; }
    }
}