using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageStorage.Migrations
{
    public partial class firstmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImagesDetails",
                columns: table => new
                {
                    ImageDetailsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagesDetails", x => x.ImageDetailsId);
                });

            migrationBuilder.CreateTable(
                name: "ImagesInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Hash = table.Column<string>(type: "TEXT", nullable: true),
                    ImageDetailsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagesInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagesInfo_ImagesDetails_ImageDetailsId",
                        column: x => x.ImageDetailsId,
                        principalTable: "ImagesDetails",
                        principalColumn: "ImageDetailsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecognizedCategories",
                columns: table => new
                {
                    ObjectId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageInfoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    ImageInformationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognizedCategories", x => x.ObjectId);
                    table.ForeignKey(
                        name: "FK_RecognizedCategories_ImagesInfo_ImageInformationId",
                        column: x => x.ImageInformationId,
                        principalTable: "ImagesInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImagesInfo_ImageDetailsId",
                table: "ImagesInfo",
                column: "ImageDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_RecognizedCategories_ImageInformationId",
                table: "RecognizedCategories",
                column: "ImageInformationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecognizedCategories");

            migrationBuilder.DropTable(
                name: "ImagesInfo");

            migrationBuilder.DropTable(
                name: "ImagesDetails");
        }
    }
}
