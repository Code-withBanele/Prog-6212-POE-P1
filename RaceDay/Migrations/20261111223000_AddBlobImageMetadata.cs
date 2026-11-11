using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaceDay.Data;

#nullable disable

namespace RaceDay.Migrations;

[DbContext(typeof(RaceDayContext))]
[Migration("20261111223000_AddBlobImageMetadata")]
public partial class AddBlobImageMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BannerBlobName",
            table: "Events",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ProfilePictureBlobName",
            table: "Participants",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BannerBlobName", table: "Events");
        migrationBuilder.DropColumn(name: "ProfilePictureBlobName", table: "Participants");
    }
}
