using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vibetech.Educat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddContactInformationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactInformation",
                table: "AspNetUsers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5653));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5660));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5663));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5666));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5668));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5671));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5674));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5676));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5679));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 23, 7, 45, 437, DateTimeKind.Utc).AddTicks(5681));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactInformation",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5631));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5640));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5643));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5645));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5648));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5650));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5653));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5656));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5659));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 22, 0, 38, 506, DateTimeKind.Utc).AddTicks(5663));
        }
    }
}
