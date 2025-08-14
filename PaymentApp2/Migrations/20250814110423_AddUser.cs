using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PaymentApp2.Migrations
{
    /// <inheritdoc />
    public partial class AddUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsRecurring = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecurrenceType = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "UserName" },
                values: new object[] { 1, "john@example.com", "john_doe" });

            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[] { "Id", "Amount", "DueDate", "IsRecurring", "Name", "Notes", "PaidDate", "RecurrenceType", "UserId" },
                values: new object[,]
                {
                    { 1, 1200.00m, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Local), true, "Rent", "Monthly rent payment", null, 1, 1 },
                    { 2, 85.50m, new DateTime(2025, 8, 29, 0, 0, 0, 0, DateTimeKind.Local), true, "Electric Bill", "Utility bill - account #12345", null, 1, 1 },
                    { 3, 15.99m, new DateTime(2025, 8, 12, 0, 0, 0, 0, DateTimeKind.Local), true, "Netflix Subscription", "Premium plan", new DateTime(2025, 8, 9, 0, 0, 0, 0, DateTimeKind.Local), 1, 1 },
                    { 4, 450.00m, new DateTime(2025, 8, 9, 0, 0, 0, 0, DateTimeKind.Local), false, "Car Insurance", "Quarterly payment - due soon!", null, 0, 1 },
                    { 5, 65.00m, new DateTime(2025, 8, 24, 0, 0, 0, 0, DateTimeKind.Local), true, "Phone Bill", "Verizon - unlimited plan", null, 1, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
