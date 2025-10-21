using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogHybrid.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addIsNSFW : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNSFW",
                table: "Communities",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNSFW",
                table: "Communities");
        }
    }
}
