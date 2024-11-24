using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace textpop_server.Migrations
{
    public partial class _102 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FCMToken",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FCMToken",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                nullable: false,
                defaultValue: "");
        }
    }
}
