using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgitekBackend.Migrations
{
    /// <inheritdoc />
    public partial class add_provider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Providers",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Providers",
                table: "Users");
        }
    }
}
