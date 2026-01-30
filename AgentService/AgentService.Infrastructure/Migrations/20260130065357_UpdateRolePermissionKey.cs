using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRolePermissionKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_permissions_permissions_permission_code",
                table: "role_permissions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_role_permissions",
                table: "role_permissions");

            migrationBuilder.DropIndex(
                name: "ix_role_permissions_permission_code",
                table: "role_permissions");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_permissions_code",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "permission_code",
                table: "role_permissions");

            migrationBuilder.AddColumn<Guid>(
                name: "permission_id",
                table: "role_permissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "pk_role_permissions",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" });

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.AddForeignKey(
                name: "fk_role_permissions_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id",
                principalTable: "permissions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_permissions_permissions_permission_id",
                table: "role_permissions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_role_permissions",
                table: "role_permissions");

            migrationBuilder.DropIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions");

            migrationBuilder.DropColumn(
                name: "permission_id",
                table: "role_permissions");

            migrationBuilder.AddColumn<string>(
                name: "permission_code",
                table: "role_permissions",
                type: "character varying(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "permissions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_role_permissions",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_code" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_permissions_code",
                table: "permissions",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_code",
                table: "role_permissions",
                column: "permission_code");

            migrationBuilder.AddForeignKey(
                name: "fk_role_permissions_permissions_permission_code",
                table: "role_permissions",
                column: "permission_code",
                principalTable: "permissions",
                principalColumn: "code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
