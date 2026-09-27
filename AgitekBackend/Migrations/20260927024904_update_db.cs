using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgitekBackend.Migrations
{
    /// <inheritdoc />
    public partial class update_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderTrackingLogs_LocationCoordinates_LocationCoordinatesId",
                table: "OrderTrackingLogs");

            migrationBuilder.DropTable(
                name: "LocationCoordinates");

            migrationBuilder.DropTable(
                name: "ProductVoucher");

            migrationBuilder.DropTable(
                name: "UserVoucher");

            migrationBuilder.RenameColumn(
                name: "PerUserLimit",
                table: "Vouchers",
                newName: "InitialQuantity");

            migrationBuilder.RenameColumn(
                name: "LocationCoordinatesId",
                table: "OrderTrackingLogs",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderTrackingLogs_LocationCoordinatesId",
                table: "OrderTrackingLogs",
                newName: "IX_OrderTrackingLogs_LocationId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Vouchers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Vouchers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    IsWarehouse = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_ProductId",
                table: "Vouchers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_UserId",
                table: "Vouchers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LocationCode",
                table: "Locations",
                column: "LocationCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTrackingLogs_Locations_LocationId",
                table: "OrderTrackingLogs",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Products_ProductId",
                table: "Vouchers",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_Users_UserId",
                table: "Vouchers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderTrackingLogs_Locations_LocationId",
                table: "OrderTrackingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Products_ProductId",
                table: "Vouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_Users_UserId",
                table: "Vouchers");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_ProductId",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_UserId",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "InitialQuantity",
                table: "Vouchers",
                newName: "PerUserLimit");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "OrderTrackingLogs",
                newName: "LocationCoordinatesId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderTrackingLogs_LocationId",
                table: "OrderTrackingLogs",
                newName: "IX_OrderTrackingLogs_LocationCoordinatesId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Vouchers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Vouchers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateTable(
                name: "LocationCoordinates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsWarehouse = table.Column<bool>(type: "bit", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    LocationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationCoordinates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductVoucher",
                columns: table => new
                {
                    ProductsId = table.Column<int>(type: "int", nullable: false),
                    VouchersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVoucher", x => new { x.ProductsId, x.VouchersId });
                    table.ForeignKey(
                        name: "FK_ProductVoucher_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductVoucher_Vouchers_VouchersId",
                        column: x => x.VouchersId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVoucher",
                columns: table => new
                {
                    UsersId = table.Column<int>(type: "int", nullable: false),
                    VouchersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVoucher", x => new { x.UsersId, x.VouchersId });
                    table.ForeignKey(
                        name: "FK_UserVoucher_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVoucher_Vouchers_VouchersId",
                        column: x => x.VouchersId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationCoordinates_LocationCode",
                table: "LocationCoordinates",
                column: "LocationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVoucher_VouchersId",
                table: "ProductVoucher",
                column: "VouchersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVoucher_VouchersId",
                table: "UserVoucher",
                column: "VouchersId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTrackingLogs_LocationCoordinates_LocationCoordinatesId",
                table: "OrderTrackingLogs",
                column: "LocationCoordinatesId",
                principalTable: "LocationCoordinates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
