namespace VakifBankVirtualPOS.WebAPI.Data.Entities
{
    public class IDT_API_KEY
    {
        public int Id { get; set; }
        public string ApiKey { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }
}