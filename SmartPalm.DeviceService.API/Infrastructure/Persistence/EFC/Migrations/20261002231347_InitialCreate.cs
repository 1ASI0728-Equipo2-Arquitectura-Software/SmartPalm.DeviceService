using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartPalm.DeviceService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "device");

            migrationBuilder.CreateTable(
                name: "edge_gateways",
                schema: "device",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mac_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    monitoring_zone_id = table.Column<long>(type: "bigint", nullable: false),
                    last_connectivity_check_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_sync_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_edge_gateways", x => x.id);
                    table.UniqueConstraint("ak_edge_gateways_mac_address", x => x.mac_address);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "device",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iot_devices",
                schema: "device",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mac_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    gateway_mac_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plantation_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iot_devices", x => x.id);
                    table.ForeignKey(
                        name: "fk_iot_devices_edge_gateways_gateway_mac_address",
                        column: x => x.gateway_mac_address,
                        principalSchema: "device",
                        principalTable: "edge_gateways",
                        principalColumn: "mac_address",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_edge_gateways_mac_address",
                schema: "device",
                table: "edge_gateways",
                column: "mac_address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_edge_gateways_monitoring_zone_id",
                schema: "device",
                table: "edge_gateways",
                column: "monitoring_zone_id");

            migrationBuilder.CreateIndex(
                name: "ix_iot_devices_gateway_mac_address",
                schema: "device",
                table: "iot_devices",
                column: "gateway_mac_address");

            migrationBuilder.CreateIndex(
                name: "ix_iot_devices_mac_address",
                schema: "device",
                table: "iot_devices",
                column: "mac_address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_iot_devices_plantation_id",
                schema: "device",
                table: "iot_devices",
                column: "plantation_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on_utc_occurred_on_utc",
                schema: "device",
                table: "outbox_messages",
                columns: new[] { "processed_on_utc", "occurred_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "iot_devices",
                schema: "device");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "device");

            migrationBuilder.DropTable(
                name: "edge_gateways",
                schema: "device");
        }
    }
}
