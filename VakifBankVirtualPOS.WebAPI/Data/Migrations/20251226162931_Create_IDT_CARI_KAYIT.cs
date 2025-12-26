using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VakifBankVirtualPOS.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class Create_IDT_CARI_KAYIT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientCode",
                table: "IDT_VAKIFBANK_ODEME",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentNo",
                table: "IDT_VAKIFBANK_ODEME",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KAYIT_KULL",
                table: "IDT_CARI_HAREKET",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "HAREKET_TIPI",
                table: "IDT_CARI_HAREKET",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "BELGE_NO",
                table: "IDT_CARI_HAREKET",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "BAKIYE",
                table: "IDT_CARI_HAREKET",
                type: "money",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "money");

            migrationBuilder.AlterColumn<string>(
                name: "ACIKLAMA",
                table: "IDT_CARI_HAREKET",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateTable(
                name: "IDT_CARI_KAYIT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CARI_KOD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CARI_ISIM = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    VERGI_DAIRESI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VERGI_NUMARASI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TCKIMLIKNO = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    CARI_ADRES = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CARI_IL = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CARI_ILCE = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EMAIL = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CARI_TEL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BAKIYE = table.Column<double>(type: "float", nullable: true),
                    SUBE_CARI_KOD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDT_CARI_KAYIT", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IDT_CARI_KAYIT");

            migrationBuilder.DropColumn(
                name: "ClientCode",
                table: "IDT_VAKIFBANK_ODEME");

            migrationBuilder.DropColumn(
                name: "DocumentNo",
                table: "IDT_VAKIFBANK_ODEME");

            migrationBuilder.AlterColumn<string>(
                name: "KAYIT_KULL",
                table: "IDT_CARI_HAREKET",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HAREKET_TIPI",
                table: "IDT_CARI_HAREKET",
                type: "varchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BELGE_NO",
                table: "IDT_CARI_HAREKET",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BAKIYE",
                table: "IDT_CARI_HAREKET",
                type: "money",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "money",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ACIKLAMA",
                table: "IDT_CARI_HAREKET",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}