using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_quotas",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monthly_token_limit = table.Column<long>(type: "bigint", nullable: false),
                    used_tokens = table.Column<long>(type: "bigint", nullable: false),
                    last_reset_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_quotas", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_quotas_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_quotas");
        }
    }
}
