using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VakifBankVirtualPOS.WebAPI.data.migrations
{
    /// <inheritdoc />
    public partial class UpdateApiKeyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "IDT_API_KEY",
                type: "nvarchar(350)",
                maxLength: 350,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "IDT_API_KEY");
        }
    }
}
