using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogHybrid.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class comunitycategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Communities_AspNetUsers_CreatorId",
                table: "Communities");

            migrationBuilder.DropForeignKey(
                name: "FK_Communities_Categories_CategoryId",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_CategoryId",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_CategoryId_IsActive_IsDeleted",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_CreatedAt",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_IsActive",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_IsDeleted",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_IsPrivate",
                table: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Communities_Slug",
                table: "Communities");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Communities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Rules",
                table: "Communities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Communities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Communities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Communities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "Communities",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Categories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7);

            migrationBuilder.CreateTable(
                name: "CommunityCategories",
                columns: table => new
                {
                    CommunityId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityCategories", x => new { x.CommunityId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_CommunityCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityCategories_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCategories_AssignedAt",
                table: "CommunityCategories",
                column: "AssignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCategories_CategoryId",
                table: "CommunityCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCategories_CommunityId",
                table: "CommunityCategories",
                column: "CommunityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_AspNetUsers_CreatorId",
                table: "Communities",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Communities_AspNetUsers_CreatorId",
                table: "Communities");

            migrationBuilder.DropTable(
                name: "CommunityCategories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Communities",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Rules",
                table: "Communities",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Communities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Communities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Communities",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CoverImageUrl",
                table: "Communities",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Categories",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Categories",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Communities_CategoryId",
                table: "Communities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_CategoryId_IsActive_IsDeleted",
                table: "Communities",
                columns: new[] { "CategoryId", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Communities_CreatedAt",
                table: "Communities",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_IsActive",
                table: "Communities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_IsDeleted",
                table: "Communities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_IsPrivate",
                table: "Communities",
                column: "IsPrivate");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_Slug",
                table: "Communities",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_AspNetUsers_CreatorId",
                table: "Communities",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_Categories_CategoryId",
                table: "Communities",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
