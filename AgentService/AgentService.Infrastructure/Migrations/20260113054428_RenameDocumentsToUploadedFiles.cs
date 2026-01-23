using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDocumentsToUploadedFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_document_chunks_documents_document_id",
                table: "document_chunks");

            migrationBuilder.DropForeignKey(
                name: "fk_documents_users_uploaded_by_user_id",
                table: "documents");

            migrationBuilder.DropPrimaryKey(
                name: "pk_documents",
                table: "documents");

            migrationBuilder.DropPrimaryKey(
                name: "pk_document_chunks",
                table: "document_chunks");

            migrationBuilder.RenameTable(
                name: "documents",
                newName: "uploaded_files");

            migrationBuilder.RenameTable(
                name: "document_chunks",
                newName: "uploaded_file_chunks");

            migrationBuilder.RenameIndex(
                name: "ix_documents_uploaded_by_user_id",
                table: "uploaded_files",
                newName: "ix_uploaded_files_uploaded_by_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_documents_category",
                table: "uploaded_files",
                newName: "ix_uploaded_files_category");

            migrationBuilder.RenameIndex(
                name: "ix_document_chunks_document_id",
                table: "uploaded_file_chunks",
                newName: "ix_uploaded_file_chunks_document_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_uploaded_files",
                table: "uploaded_files",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_uploaded_file_chunks",
                table: "uploaded_file_chunks",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_uploaded_file_chunks_uploaded_files_document_id",
                table: "uploaded_file_chunks",
                column: "document_id",
                principalTable: "uploaded_files",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_uploaded_files_users_uploaded_by_user_id",
                table: "uploaded_files",
                column: "uploaded_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_uploaded_file_chunks_uploaded_files_document_id",
                table: "uploaded_file_chunks");

            migrationBuilder.DropForeignKey(
                name: "fk_uploaded_files_users_uploaded_by_user_id",
                table: "uploaded_files");

            migrationBuilder.DropPrimaryKey(
                name: "pk_uploaded_files",
                table: "uploaded_files");

            migrationBuilder.DropPrimaryKey(
                name: "pk_uploaded_file_chunks",
                table: "uploaded_file_chunks");

            migrationBuilder.RenameTable(
                name: "uploaded_files",
                newName: "documents");

            migrationBuilder.RenameTable(
                name: "uploaded_file_chunks",
                newName: "document_chunks");

            migrationBuilder.RenameIndex(
                name: "ix_uploaded_files_uploaded_by_user_id",
                table: "documents",
                newName: "ix_documents_uploaded_by_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_uploaded_files_category",
                table: "documents",
                newName: "ix_documents_category");

            migrationBuilder.RenameIndex(
                name: "ix_uploaded_file_chunks_document_id",
                table: "document_chunks",
                newName: "ix_document_chunks_document_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_documents",
                table: "documents",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_document_chunks",
                table: "document_chunks",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_document_chunks_documents_document_id",
                table: "document_chunks",
                column: "document_id",
                principalTable: "documents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_documents_users_uploaded_by_user_id",
                table: "documents",
                column: "uploaded_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
