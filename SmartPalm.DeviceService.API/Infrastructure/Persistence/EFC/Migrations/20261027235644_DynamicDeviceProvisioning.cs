using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPalm.DeviceService.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class DynamicDeviceProvisioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bootstrap_credential_hash",
                schema: "device",
                table: "iot_devices",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "device",
                table: "iot_devices",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_provisioned",
                schema: "device",
                table: "iot_devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "provisioned_at",
                schema: "device",
                table: "iot_devices",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bootstrap_credential_hash",
                schema: "device",
                table: "iot_devices");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "device",
                table: "iot_devices");

            migrationBuilder.DropColumn(
                name: "is_provisioned",
                schema: "device",
                table: "iot_devices");

            migrationBuilder.DropColumn(
                name: "provisioned_at",
                schema: "device",
                table: "iot_devices");
        }
    }
}
