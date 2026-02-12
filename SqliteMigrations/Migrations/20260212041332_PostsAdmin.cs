using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqliteMigrations.Migrations
{
    /// <inheritdoc />
    public partial class PostsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "Attachment",
                newName: "Slug");

            migrationBuilder.RenameIndex(
                name: "IX_Attachment_Guid",
                table: "Attachment",
                newName: "IX_Attachment_Slug");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Post",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Alt",
                table: "Attachment",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Attachment",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Linkedin",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 280,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Twitter",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 280,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "Alt",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Linkedin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Twitter",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Attachment",
                newName: "Guid");

            migrationBuilder.RenameIndex(
                name: "IX_Attachment_Slug",
                table: "Attachment",
                newName: "IX_Attachment_Guid");
        }
    }
}
