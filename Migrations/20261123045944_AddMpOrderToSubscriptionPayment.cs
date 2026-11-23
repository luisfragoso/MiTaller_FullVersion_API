using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class AddMpOrderToSubscriptionPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountCodeId",
                table: "SubscriptionPayments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountSourceId",
                table: "SubscriptionPayments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MpOrderId",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountCodeId",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "DiscountSourceId",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "MpOrderId",
                table: "SubscriptionPayments");
        }
    }
}
