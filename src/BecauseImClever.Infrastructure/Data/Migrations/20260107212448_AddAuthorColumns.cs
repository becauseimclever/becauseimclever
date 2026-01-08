using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BecauseImClever.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "author_id",
                table: "posts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "author_name",
                table: "posts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_posts_author_id",
                table: "posts",
                column: "author_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_posts_author_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "author_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "author_name",
                table: "posts");
        }
    }
}
