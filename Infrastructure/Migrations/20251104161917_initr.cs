using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Models_Scissors_ScissorID",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_Models_ScissorID",
                table: "Models");

            migrationBuilder.DropColumn(
                name: "ScissorID",
                table: "Models");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Scissors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Scissors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Scissors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Scissors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "ScissorID",
                table: "Models",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Models_ScissorID",
                table: "Models",
                column: "ScissorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Models_Scissors_ScissorID",
                table: "Models",
                column: "ScissorID",
                principalTable: "Scissors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
