using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace whobe.Migrations
{
    /// <inheritdoc />
    public partial class CheckMessageReadReceiptRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReadReceipt_Messages_MessageId1",
                table: "MessageReadReceipt");

            migrationBuilder.DropIndex(
                name: "IX_MessageReadReceipt_MessageId1",
                table: "MessageReadReceipt");

            migrationBuilder.DropColumn(
                name: "MessageId1",
                table: "MessageReadReceipt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MessageId1",
                table: "MessageReadReceipt",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipt_MessageId1",
                table: "MessageReadReceipt",
                column: "MessageId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadReceipt_Messages_MessageId1",
                table: "MessageReadReceipt",
                column: "MessageId1",
                principalTable: "Messages",
                principalColumn: "MessageId");
        }
    }
}
