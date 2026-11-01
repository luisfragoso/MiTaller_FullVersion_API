using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class ReviewRepliesAndHelpfulVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkshopReply",
                table: "Reviews",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkshopReplyDate",
                table: "Reviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReviewHelpfulVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewHelpfulVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewHelpfulVotes_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHelpfulVotes_ReviewId_CustomerId",
                table: "ReviewHelpfulVotes",
                columns: new[] { "ReviewId", "CustomerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewHelpfulVotes");

            migrationBuilder.DropColumn(
                name: "WorkshopReply",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "WorkshopReplyDate",
                table: "Reviews");
        }
    }
}
