using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BecauseImClever.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledPublishDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "scheduled_publish_date",
                table: "posts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "scheduled_publish_date",
                table: "posts");
        }
    }
}
