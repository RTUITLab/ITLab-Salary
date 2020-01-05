using Microsoft.EntityFrameworkCore.Migrations;

namespace ITLab.Salary.Backend.Migrations
{
    public partial class EventIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EventSalaries_EventId",
                table: "EventSalaries",
                column: "EventId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventSalaries_EventId",
                table: "EventSalaries");
        }
    }
}
