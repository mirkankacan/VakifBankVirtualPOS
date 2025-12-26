using Mapster;
using VakifBankVirtualPOS.WebAPI.Data.Entities;
using VakifBankVirtualPOS.WebAPI.Dtos.ClientDtos;
using VakifBankVirtualPOS.WebAPI.Dtos.HybsDtos;

namespace VakifBankVirtualPOS.WebAPI.Mappings
{
    public class ClientMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // IDT_CARI_KAYIT -> ClientDto mapping
            config.NewConfig<IDT_CARI_KAYIT, ClientDto>()
                .Map(dest => dest, src => src);

            // IDT_CARI_HAREKET -> DocumentProcessList mapping
            config.NewConfig<IDT_CARI_HAREKET, DocumentProcessList>()
                .Map(dest => dest.OrderNo, src => src.ACIKLAMA)
                .Map(dest => dest.Amount, src => src.BORC)
                .Map(dest => dest.Date, src => src.TARIH);

            // List<IDT_CARI_HAREKET> -> DocumentCheckDto mapping
            config.NewConfig<List<IDT_CARI_HAREKET>, DocumentCheckDto>()
                .Map(dest => dest.DocumentProcessList, src => src != null ? src.Adapt<List<DocumentProcessList>>() : new List<DocumentProcessList>())
                .Map(dest => dest.DocumentProcessCount, src => src != null ? src.Count : 0)
                .Map(dest => dest.IsDocumentExist, src => src != null && src.Count > 0)
                .Map(dest => dest.DocumentNo, src => src != null && src.Any() ? (src.First().BELGE_NO ?? string.Empty) : string.Empty);
        }
    }
}