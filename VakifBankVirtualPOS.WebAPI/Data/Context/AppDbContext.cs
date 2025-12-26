using Microsoft.EntityFrameworkCore;
using VakifBankVirtualPOS.WebAPI.Data.Entities;

namespace VakifBankVirtualPOS.WebAPI.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<IDT_CARI_HAREKET> IDT_CARI_HAREKET => Set<IDT_CARI_HAREKET>();
        public DbSet<IDT_VAKIFBANK_ODEME> IDT_VAKIFBANK_ODEME => Set<IDT_VAKIFBANK_ODEME>();
        public DbSet<IDT_CARI_KAYIT> IDT_CARI_KAYIT => Set<IDT_CARI_KAYIT>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IDT_CARI_KAYIT>(entity =>
            {
                entity.ToTable("IDT_CARI_KAYIT");

                entity.HasKey(e => e.ID);

                entity.Property(e => e.CARI_KOD)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CARI_ISIM)
                 .IsRequired()
                 .HasMaxLength(350);

                entity.Property(e => e.VERGI_DAIRESI)
               .IsRequired(false)
               .HasMaxLength(50);
                entity.Property(e => e.VERGI_NUMARASI)
            .IsRequired(false)
            .HasMaxLength(50);

                entity.Property(e => e.TCKIMLIKNO)
            .IsRequired(false)
            .HasMaxLength(11);

                entity.Property(e => e.CARI_ADRES)
            .IsRequired(false)
            .HasMaxLength(500);

                entity.Property(e => e.CARI_IL)
            .IsRequired(false)
            .HasMaxLength(150);
                entity.Property(e => e.CARI_ILCE)
            .IsRequired(false)
            .HasMaxLength(150);

                entity.Property(e => e.EMAIL)
            .IsRequired(false)
            .HasMaxLength(250);

                entity.Property(e => e.CARI_TEL)
            .IsRequired(false)
            .HasMaxLength(50);
                entity.Property(e => e.BAKIYE)
            .IsRequired(false)
            .HasColumnType("float");

                entity.Property(e => e.SUBE_CARI_KOD)
            .IsRequired(false)
            .HasMaxLength(50);
            });
            modelBuilder.Entity<IDT_CARI_HAREKET>(entity =>
            {
                entity.ToTable("IDT_CARI_HAREKET");

                entity.HasKey(e => e.ID);

                entity.Property(e => e.CARI_KODU)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TARIH)
                    .IsRequired()
                    .HasColumnType("datetime");

                entity.Property(e => e.BELGE_NO)
                    .HasMaxLength(50).IsRequired(false);

                entity.Property(e => e.ACIKLAMA)
                    .HasMaxLength(250).IsRequired(false);

                entity.Property(e => e.BORC)
                    .IsRequired()
                    .HasColumnType("money");

                entity.Property(e => e.ALACAK)
                    .IsRequired()
                    .HasColumnType("money");

                entity.Property(e => e.BAKIYE)
                    .HasColumnType("money").IsRequired(false);

                entity.Property(e => e.HAREKET_TIPI)
                    .HasMaxLength(1)
                    .HasColumnType("varchar(1)")
                    .IsRequired(false);

                entity.Property(e => e.KAYIT_KULL)
                    .HasMaxLength(50).IsRequired(false);

                entity.Property(e => e.KAYIT_ZAMAN)
                    .IsRequired()
                    .HasColumnType("datetime");

                entity.Property(e => e.AKTARIM)
                    .IsRequired();
            });

            modelBuilder.Entity<IDT_VAKIFBANK_ODEME>(entity =>
        {
            entity.ToTable("IDT_VAKIFBANK_ODEME");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TransactionId)
                .HasMaxLength(100);

            entity.Property(e => e.AuthCode)
                .HasMaxLength(50);

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.MaskedCardNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.CardHolderName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CardBrand)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.Property(e => e.ErrorCode)
                .HasMaxLength(50);

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500);

            entity.Property(e => e.ThreeDSecureStatus)
                .HasMaxLength(1);

            entity.Property(e => e.ClientIp)
                .IsRequired()
                .HasMaxLength(45);

            entity.Property(e => e.UserId);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.ClientCode)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.DocumentNo)
                  .IsRequired(false)
                  .HasMaxLength(50);
        });
        }
    }
}