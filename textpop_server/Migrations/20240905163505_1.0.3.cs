using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace textpop_server.Migrations
{
    public partial class _103 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockedInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InitiatedUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BlockedUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedInfo_AspNetUsers_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_BlockedInfo_AspNetUsers_InitiatedUserId",
                        column: x => x.InitiatedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedInfo_BlockedUserId",
                table: "BlockedInfo",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedInfo_InitiatedUserId",
                table: "BlockedInfo",
                column: "InitiatedUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedInfo");
        }
    }
}
