using Mapster;
using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Dtos.PaymentDtos;

namespace VakifBankVirtualPOS.WebAPI.Mappings
{
    public class PaymentMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<IDT_VAKIFBANK_ODEME, PaymentResultDto>()
                .Map(dest => dest.OrderId, src => src.OrderId)
                .Map(dest => dest.ResultCode, src => src.ResultCode)
                .Map(dest => dest.Message, src => src.ErrorMessage)
                .Map(dest => dest.AuthCode, src => src.AuthCode)
                .Map(dest => dest.TransactionId, src => src.TransactionId);
        }
    }
}