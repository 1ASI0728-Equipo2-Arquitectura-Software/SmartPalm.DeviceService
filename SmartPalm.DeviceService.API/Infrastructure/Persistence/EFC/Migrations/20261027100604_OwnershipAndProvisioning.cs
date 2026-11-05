using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPalm.DeviceService.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class OwnershipAndProvisioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "owner_user_id",
                schema: "device",
                table: "iot_devices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "sector_id",
                schema: "device",
                table: "iot_devices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "credential_hash",
                schema: "device",
                table: "edge_gateways",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "device",
                table: "edge_gateways",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "owner_user_id",
                schema: "device",
                table: "edge_gateways",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_iot_devices_plantation_id_sector_id",
                schema: "device",
                table: "iot_devices",
                columns: new[] { "plantation_id", "sector_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_iot_devices_plantation_id_sector_id",
                schema: "device",
                table: "iot_devices");

            migrationBuilder.DropColumn(
                name: "owner_user_id",
                schema: "device",
                table: "iot_devices");

            migrationBuilder.DropColumn(
                name: "sector_id",
                schema: "device",
                table: "iot_devices");

            migrationBuilder.DropColumn(
                name: "credential_hash",
                schema: "device",
                table: "edge_gateways");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "device",
                table: "edge_gateways");

            migrationBuilder.DropColumn(
                name: "owner_user_id",
                schema: "device",
                table: "edge_gateways");
        }
    }
}
