using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentACar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email",
                table: "Drivers",
                newName: "Email");

            //migrationBuilder.AddColumn<bool>(
            //    name: "RequiresHumanIntervention",
            //    table: "SupportConversations",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AiConversations",
                columns: table => new
                {
                    AiConversationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEscalated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiConversations", x => x.AiConversationId);
                    table.ForeignKey(
                        name: "FK_AiConversations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiMessages",
                columns: table => new
                {
                    AiMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AiConversationId = table.Column<int>(type: "int", nullable: false),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiMessages", x => x.AiMessageId);
                    table.ForeignKey(
                        name: "FK_AiMessages_AiConversations_AiConversationId",
                        column: x => x.AiConversationId,
                        principalTable: "AiConversations",
                        principalColumn: "AiConversationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_CustomerId",
                table: "AiConversations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_LastActiveAt",
                table: "AiConversations",
                column: "LastActiveAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessages_AiConversationId",
                table: "AiMessages",
                column: "AiConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiMessages");

            migrationBuilder.DropTable(
                name: "AiConversations");

            migrationBuilder.DropColumn(
                name: "RequiresHumanIntervention",
                table: "SupportConversations");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Drivers",
                newName: "email");
        }
    }
}
