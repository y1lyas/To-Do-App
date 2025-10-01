using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoApp.Migrations
{
    /// <inheritdoc />
    public partial class CreatedByIdFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var systemUserId = new Guid("00000000-0000-0000-0000-000000000001");
            var CreatedTuime = DateTime.UtcNow;

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Username", "Email", "PasswordHash", "CreatedAt" },
                values: new object[] { systemUserId, "System", "system@system.local", "SYSTEM_HASH", CreatedTuime }
            );

            // Eski görevlerin CreatedById kolonunu System User'a bağla
            migrationBuilder.Sql($@"
        UPDATE ""Tasks""
        SET ""CreatedById"" = '{systemUserId}'
        WHERE ""CreatedById"" IS NULL
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
