using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CommunityWiki.Migrations
{
    public partial class UserApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedOn",
                table: "Users",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JoinedOn",
                table: "Users");
        }
    }
}
