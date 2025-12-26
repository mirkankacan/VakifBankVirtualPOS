using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VakifBankVirtualPOS.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IDT_CARI_HAREKET",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CARI_KODU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TARIH = table.Column<DateTime>(type: "datetime", nullable: false),
                    BELGE_NO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ACIKLAMA = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    BORC = table.Column<decimal>(type: "money", nullable: false),
                    ALACAK = table.Column<decimal>(type: "money", nullable: false),
                    BAKIYE = table.Column<decimal>(type: "money", nullable: false),
                    HAREKET_TIPI = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false),
                    KAYIT_KULL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KAYIT_ZAMAN = table.Column<DateTime>(type: "datetime", nullable: false),
                    AKTARIM = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDT_CARI_HAREKET", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "IDT_VAKIFBANK_ODEME",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuthCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    MaskedCardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CardHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CardBrand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThreeDSecureStatus = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    ClientIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDT_VAKIFBANK_ODEME", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IDT_CARI_HAREKET");

            migrationBuilder.DropTable(
                name: "IDT_VAKIFBANK_ODEME");
        }
    }
}
