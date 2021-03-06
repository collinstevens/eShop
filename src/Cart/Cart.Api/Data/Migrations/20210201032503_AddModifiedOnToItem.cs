using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cart.Api.Data.Migrations
{
    public partial class AddModifiedOnToItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "Items",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "Items");
        }
    }
}
