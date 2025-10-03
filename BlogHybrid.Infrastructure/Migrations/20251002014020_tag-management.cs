using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogHybrid.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class tagmanagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Tags",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CreatedBy",
                table: "Tags",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_AspNetUsers_CreatedBy",
                table: "Tags",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_AspNetUsers_CreatedBy",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_CreatedBy",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tags");
        }
    }
}
