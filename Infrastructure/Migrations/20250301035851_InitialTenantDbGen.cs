using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTenantDbGen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Multitenancy");

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "Multitenancy",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Identifier = table.Column<string>(type: "varchar(450)", nullable: false),
                    Name = table.Column<string>(type: "varchar(60)", nullable: false),
                    ConnectionString = table.Column<string>(type: "varchar(450)", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", nullable: true),
                    FirsName = table.Column<string>(type: "varchar(60)", nullable: false),
                    LastName = table.Column<string>(type: "varchar(60)", nullable: true),
                    ValidUpTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Identifier",
                schema: "Multitenancy",
                table: "Tenants",
                column: "Identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "Multitenancy");
        }
    }
}
