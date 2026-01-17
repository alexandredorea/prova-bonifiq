using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProvaPub.Infrastructure.Persistences.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("IX_Customers_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Numbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Numbers_Id", x => x.Id);
                    table.CheckConstraint("CK_Numbers_Number_Range", "[Number] >= 0 AND [Number] <= 99");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("IX_Products_Id", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("IX_Orders_Id", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_Id",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Christie Roberts" },
                    { 2, "Rafael McClure" },
                    { 3, "Nathan Veum" },
                    { 4, "Alfredo Hodkiewicz" },
                    { 5, "Jason Legros" },
                    { 6, "Erick Kshlerin" },
                    { 7, "Sylvester Bauch" },
                    { 8, "Allison McGlynn" },
                    { 9, "Lorraine Mitchell" },
                    { 10, "Jessie Cummerata" },
                    { 11, "Madeline Steuber" },
                    { 12, "Gilberto Blanda" },
                    { 13, "Elbert Langworth" },
                    { 14, "Mercedes Schaden" },
                    { 15, "Milton Conroy" },
                    { 16, "Myrtle Dickens" },
                    { 17, "Dan Robel" },
                    { 18, "Gene Goodwin" },
                    { 19, "Harvey Bernier" },
                    { 20, "Mae Thiel" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Licensed Metal Bike" },
                    { 2, "Incredible Cotton Chair" },
                    { 3, "Awesome Fresh Salad" },
                    { 4, "Incredible Cotton Soap" },
                    { 5, "Refined Granite Keyboard" },
                    { 6, "Fantastic Cotton Computer" },
                    { 7, "Licensed Fresh Pizza" },
                    { 8, "Generic Soft Chair" },
                    { 9, "Generic Cotton Fish" },
                    { 10, "Generic Plastic Towels" },
                    { 11, "Handmade Fresh Pizza" },
                    { 12, "Unbranded Fresh Hat" },
                    { 13, "Practical Granite Pizza" },
                    { 14, "Unbranded Steel Pants" },
                    { 15, "Handcrafted Frozen Chair" },
                    { 16, "Practical Wooden Keyboard" },
                    { 17, "Incredible Steel Hat" },
                    { 18, "Awesome Concrete Fish" },
                    { 19, "Generic Rubber Mouse" },
                    { 20, "Licensed Cotton Pants" }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Numbers_Number",
                table: "Numbers",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UQ_Products_Name",
                table: "Products",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Numbers");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
