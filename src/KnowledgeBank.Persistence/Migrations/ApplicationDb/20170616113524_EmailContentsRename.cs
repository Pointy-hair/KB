using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KnowledgeBank.Persistence.Migrations.ApplicationDb
{
    public partial class EmailContentsRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contents",
                table: "Emailtemplate");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Emailtemplate",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Emailtemplate");

            migrationBuilder.AddColumn<string>(
                name: "Contents",
                table: "Emailtemplate",
                nullable: true);
        }
    }
}
