using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VakifBankVirtualPOS.WebAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVakıfBankTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "IDT_VAKIFBANK_ODEME");

            migrationBuilder.RenameColumn(
                name: "ErrorCode",
                table: "IDT_VAKIFBANK_ODEME",
                newName: "ResultCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResultCode",
                table: "IDT_VAKIFBANK_ODEME",
                newName: "ErrorCode");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "IDT_VAKIFBANK_ODEME",
                type: "int",
                nullable: true);
        }
    }
}
