using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BecauseImClever.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "commissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    base_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    options = table.Column<string>(type: "jsonb", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "commission_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    commission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filename = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: false),
                    alt_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commission_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_commission_images_commissions_commission_id",
                        column: x => x.commission_id,
                        principalTable: "commissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_commission_images_commission_id_sort_order",
                table: "commission_images",
                columns: new[] { "commission_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_commissions_slug",
                table: "commissions",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commission_images");

            migrationBuilder.DropTable(
                name: "commissions");
        }
    }
}
