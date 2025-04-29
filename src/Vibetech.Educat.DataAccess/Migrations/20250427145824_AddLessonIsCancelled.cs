using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vibetech.Educat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonIsCancelled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Lessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6197));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6210));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6214));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6217));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6221));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6224));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6227));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6230));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6233));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 14, 58, 23, 136, DateTimeKind.Utc).AddTicks(6236));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Lessons");

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4872));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4880));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4883));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4886));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4889));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4891));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4894));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4896));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4899));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 27, 12, 19, 19, 292, DateTimeKind.Utc).AddTicks(4901));
        }
    }
}
