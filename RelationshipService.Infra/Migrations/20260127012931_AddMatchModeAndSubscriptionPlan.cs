using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RelationshipService.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchModeAndSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfile_Discovery_BasicFilter",
                table: "user_profiles");

            migrationBuilder.AddColumn<int>(
                name: "match_mode",
                table: "user_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "subscription_plan",
                table: "user_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_Discovery_BasicFilter",
                table: "user_profiles",
                columns: new[] { "match_mode", "gender", "is_deleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfile_Discovery_BasicFilter",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "match_mode",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "subscription_plan",
                table: "user_profiles");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_Discovery_BasicFilter",
                table: "user_profiles",
                columns: new[] { "gender", "is_deleted" });
        }
    }
}
