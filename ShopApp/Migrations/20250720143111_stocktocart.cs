using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopApp.Migrations
{
    /// <inheritdoc />
    public partial class stocktocart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.AddColumn<int>(
                name: "StockID",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockID",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_StockID",
                table: "OrderItems",
                column: "StockID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_StockID",
                table: "CartItems",
                column: "StockID");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Stocks_StockID",
                table: "CartItems",
                column: "StockID",
                principalTable: "Stocks",
                principalColumn: "StockId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Stocks_StockID",
                table: "OrderItems",
                column: "StockID",
                principalTable: "Stocks",
                principalColumn: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Stocks_StockID",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Stocks_StockID",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_StockID",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_StockID",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "StockID",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "StockID",
                table: "CartItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
