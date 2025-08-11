using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace digital.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "TimeTables");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Attendance");

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "TimeTables",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Subject",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherMaster_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherMaster_SubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherMaster_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TeacherId",
                table: "Users",
                column: "TeacherId",
                unique: true,
                filter: "[TeacherId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTables_TeacherId",
                table: "TimeTables",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_CategoryId",
                table: "Student",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_SubCategoryId",
                table: "Student",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherMaster_CategoryId",
                table: "TeacherMaster",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherMaster_SubCategoryId",
                table: "TeacherMaster",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherMaster_SubjectId",
                table: "TeacherMaster",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Categories_CategoryId",
                table: "Student",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_SubCategories_SubCategoryId",
                table: "Student",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeTables_Users_TeacherId",
                table: "TimeTables",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_TeacherMaster_TeacherId",
                table: "Users",
                column: "TeacherId",
                principalTable: "TeacherMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Student_Categories_CategoryId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_SubCategories_SubCategoryId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeTables_Users_TeacherId",
                table: "TimeTables");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_TeacherMaster_TeacherId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "TeacherMaster");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.DropIndex(
                name: "IX_Users_TeacherId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TimeTables_TeacherId",
                table: "TimeTables");

            migrationBuilder.DropIndex(
                name: "IX_Student_CategoryId",
                table: "Student");

            migrationBuilder.DropIndex(
                name: "IX_Student_SubCategoryId",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "TimeTables");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "TimeTables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Attendance",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
