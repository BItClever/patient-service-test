using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientService.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    name_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name_use = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name_family = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name_given = table.Column<string>(type: "jsonb", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    birth_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patients");
        }
    }
}
