using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HandyRank.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "ServiceRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "XPGranted",
                table: "Reviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "RatingAverage",
                table: "HandymanProfiles",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalJobsCompleted",
                table: "HandymanProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ServiceRequestId",
                table: "Reviews",
                column: "ServiceRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_ServiceRequests_ServiceRequestId",
                table: "Reviews",
                column: "ServiceRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_ServiceRequests_ServiceRequestId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_ServiceRequestId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "XPGranted",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingAverage",
                table: "HandymanProfiles");

            migrationBuilder.DropColumn(
                name: "TotalJobsCompleted",
                table: "HandymanProfiles");
        }
    }
}
