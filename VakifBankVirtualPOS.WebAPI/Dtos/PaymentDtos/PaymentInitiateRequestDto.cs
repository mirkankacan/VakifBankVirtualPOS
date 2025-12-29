namespace VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos
{
    public class PaymentInitiateRequestDto
    {
        public string CardNumber { get; set; }

        public string ExpiryDate { get; set; } // YYMM formatında (örn: 2512)

        public string Cvv { get; set; }

        public string CardHolderName { get; set; }

        public decimal Amount { get; set; }

        public string ClientCode { get; set; }

        public string? DocumentNo { get; set; }

        //[Range(1, 12, ErrorMessage = "Taksit sayısı 1 ile 12 arasında olmalıdır")]
        //public int? InstallmentCount { get; set; } = null;
    }
}