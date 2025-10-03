using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BlogHybrid.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class tag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommunityId",
                table: "Posts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Communities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Rules = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    RequireApproval = table.Column<bool>(type: "boolean", nullable: false),
                    MemberCount = table.Column<int>(type: "integer", nullable: false),
                    PostCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Communities_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Communities_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommunityInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommunityId = table.Column<int>(type: "integer", nullable: false),
                    InviterId = table.Column<string>(type: "text", nullable: false),
                    InviteeEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    InviteeId = table.Column<string>(type: "text", nullable: true),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityInvites_AspNetUsers_InviteeId",
                        column: x => x.InviteeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityInvites_AspNetUsers_InviterId",
                        column: x => x.InviterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityInvites_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunityMembers",
                columns: table => new
                {
                    CommunityId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityMembers", x => new { x.CommunityId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CommunityMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityMembers_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CommunityId",
                table: "Posts",
                column: "CommunityId");

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
                name: "IX_Communities_CreatorId",
                table: "Communities",
                column: "CreatorId");

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

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_CommunityId",
                table: "CommunityInvites",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_CommunityId_InviteeEmail",
                table: "CommunityInvites",
                columns: new[] { "CommunityId", "InviteeEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_CreatedAt",
                table: "CommunityInvites",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_ExpiresAt",
                table: "CommunityInvites",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_InviteeEmail",
                table: "CommunityInvites",
                column: "InviteeEmail");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_InviteeId",
                table: "CommunityInvites",
                column: "InviteeId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_InviterId",
                table: "CommunityInvites",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_IsUsed",
                table: "CommunityInvites",
                column: "IsUsed");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_Token",
                table: "CommunityInvites",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityInvites_Token_IsUsed",
                table: "CommunityInvites",
                columns: new[] { "Token", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_CommunityId",
                table: "CommunityMembers",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_CommunityId_IsApproved",
                table: "CommunityMembers",
                columns: new[] { "CommunityId", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_CommunityId_Role",
                table: "CommunityMembers",
                columns: new[] { "CommunityId", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_IsApproved",
                table: "CommunityMembers",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_IsBanned",
                table: "CommunityMembers",
                column: "IsBanned");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_JoinedAt",
                table: "CommunityMembers",
                column: "JoinedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_Role",
                table: "CommunityMembers",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembers_UserId",
                table: "CommunityMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Communities_CommunityId",
                table: "Posts",
                column: "CommunityId",
                principalTable: "Communities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Communities_CommunityId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "CommunityInvites");

            migrationBuilder.DropTable(
                name: "CommunityMembers");

            migrationBuilder.DropTable(
                name: "Communities");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CommunityId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CommunityId",
                table: "Posts");
        }
    }
}
