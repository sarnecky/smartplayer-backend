using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Smartplayer.Authorization.WebApi.Migrations
{
    public partial class fieldId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FieldId",
                table: "Game",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Game_FieldId",
                table: "Game",
                column: "FieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_Field_FieldId",
                table: "Game",
                column: "FieldId",
                principalTable: "Field",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_Field_FieldId",
                table: "Game");

            migrationBuilder.DropIndex(
                name: "IX_Game_FieldId",
                table: "Game");

            migrationBuilder.DropColumn(
                name: "FieldId",
                table: "Game");
        }
    }
}
