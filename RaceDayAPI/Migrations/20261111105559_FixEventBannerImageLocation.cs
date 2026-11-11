using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class FixEventBannerImageLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "banner_image_url",
                table: "ROUTE");

            migrationBuilder.AddColumn<string>(
                name: "banner_image_url",
                table: "EVENT",
                type: "varchar(500)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "banner_image_url",
                table: "EVENT");

            migrationBuilder.AddColumn<string>(
                name: "banner_image_url",
                table: "ROUTE",
                type: "varchar(500)",
                nullable: true);
        }
    }
}
