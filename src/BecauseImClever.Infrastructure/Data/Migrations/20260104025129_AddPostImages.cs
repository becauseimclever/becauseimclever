using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BecauseImClever.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "extension_detection_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    fingerprint_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    extension_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    extension_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    detected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_address_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extension_detection_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureName = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: false),
                    DisabledReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "post_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    original_filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    alt_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    uploaded_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_post_images_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_extension_detection_events_detected_at",
                table: "extension_detection_events",
                column: "detected_at");

            migrationBuilder.CreateIndex(
                name: "IX_extension_detection_events_extension_id",
                table: "extension_detection_events",
                column: "extension_id");

            migrationBuilder.CreateIndex(
                name: "IX_extension_detection_events_fingerprint_hash",
                table: "extension_detection_events",
                column: "fingerprint_hash");

            migrationBuilder.CreateIndex(
                name: "IX_post_images_post_id",
                table: "post_images",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_images_post_id_filename",
                table: "post_images",
                columns: new[] { "post_id", "filename" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "extension_detection_events");

            migrationBuilder.DropTable(
                name: "FeatureSettings");

            migrationBuilder.DropTable(
                name: "post_images");
        }
    }
}
