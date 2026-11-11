using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RaceDay.Data;

#nullable disable

namespace RaceDay.Migrations;

[DbContext(typeof(RaceDayContext))]
[Migration("20261111214000_AddEventCategories")]
public partial class AddEventCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EventCategories",
            columns: table => new
            {
                EventsId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CategoriesId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EventCategories", x => new { x.EventsId, x.CategoriesId });
                table.ForeignKey(
                    name: "FK_EventCategories_Categories_CategoriesId",
                    column: x => x.CategoriesId,
                    principalTable: "Categories",
                    principalColumn: "CategoriesId",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_EventCategories_Events_EventsId",
                    column: x => x.EventsId,
                    principalTable: "Events",
                    principalColumn: "EventsId",
                    onDelete: ReferentialAction.Cascade);
            });

        // Preserve the category currently assigned to every existing event.
        migrationBuilder.Sql("""
            INSERT INTO EventCategories (EventsId, CategoriesId)
            SELECT EventsId, CategoriesId
            FROM Events
            WHERE CategoriesId IS NOT NULL
              AND LTRIM(RTRIM(CategoriesId)) <> '';
            """);

        migrationBuilder.CreateIndex(
            name: "IX_EventCategories_CategoriesId",
            table: "EventCategories",
            column: "CategoriesId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EventCategories");
    }
}
