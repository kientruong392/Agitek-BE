using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgitekBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderTrackingLogRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderTrackingLogs_Orders_OrderId1",
                table: "OrderTrackingLogs");

            migrationBuilder.DropIndex(
                name: "IX_OrderTrackingLogs_OrderId1",
                table: "OrderTrackingLogs");

            migrationBuilder.DropColumn(
                name: "OrderId1",
                table: "OrderTrackingLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderId1",
                table: "OrderTrackingLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderTrackingLogs_OrderId1",
                table: "OrderTrackingLogs",
                column: "OrderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTrackingLogs_Orders_OrderId1",
                table: "OrderTrackingLogs",
                column: "OrderId1",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
