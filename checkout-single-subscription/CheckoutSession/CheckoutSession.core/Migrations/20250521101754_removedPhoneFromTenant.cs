using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckoutSession.core.Migrations
{
    /// <inheritdoc />
    public partial class removedPhoneFromTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Tenants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
