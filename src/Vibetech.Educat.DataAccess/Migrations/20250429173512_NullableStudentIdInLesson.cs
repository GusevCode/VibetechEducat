using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vibetech.Educat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NullableStudentIdInLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "Lessons",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3109));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3120));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3123));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3126));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3128));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3131));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3134));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3136));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3139));

            migrationBuilder.UpdateData(
                table: "Subjects",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 4, 29, 17, 35, 11, 292, DateTimeKind.Utc).AddTicks(3141));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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
    }
}
