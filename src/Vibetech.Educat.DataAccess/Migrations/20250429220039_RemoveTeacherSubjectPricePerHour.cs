using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vibetech.Educat.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTeacherSubjectPricePerHour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "TeacherSubjects");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "TeacherSubjects",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

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
    }
}
