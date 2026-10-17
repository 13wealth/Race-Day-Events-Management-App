using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    role = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "PROFILE",
                columns: table => new
                {
                    profile_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROFILE", x => x.profile_id);
                    table.ForeignKey(
                        name: "FK_PROFILE_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ORGANISER",
                columns: table => new
                {
                    organiser_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    profile_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORGANISER", x => x.organiser_id);
                    table.ForeignKey(
                        name: "FK_ORGANISER_PROFILE_profile_id",
                        column: x => x.profile_id,
                        principalTable: "PROFILE",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PARTICIPANT",
                columns: table => new
                {
                    participant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    profile_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PARTICIPANT", x => x.participant_id);
                    table.ForeignKey(
                        name: "FK_PARTICIPANT_PROFILE_profile_id",
                        column: x => x.profile_id,
                        principalTable: "PROFILE",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EVENT",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    organiser_id = table.Column<int>(type: "int", nullable: false),
                    event_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: false),
                    event_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    distance = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    event_type = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EVENT", x => x.event_id);
                    table.ForeignKey(
                        name: "FK_EVENT_ORGANISER_organiser_id",
                        column: x => x.organiser_id,
                        principalTable: "ORGANISER",
                        principalColumn: "organiser_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CATEGORY",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    category_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATEGORY", x => x.category_id);
                    table.ForeignKey(
                        name: "FK_CATEGORY_EVENT_event_id",
                        column: x => x.event_id,
                        principalTable: "EVENT",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ROUTE",
                columns: table => new
                {
                    route_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    route_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    distance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    start_location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    end_location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    map_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROUTE", x => x.route_id);
                    table.ForeignKey(
                        name: "FK_ROUTE_EVENT_event_id",
                        column: x => x.event_id,
                        principalTable: "EVENT",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ENROLLMENT",
                columns: table => new
                {
                    enrolment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    participant_id = table.Column<int>(type: "int", nullable: false),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    entry_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENROLLMENT", x => x.enrolment_id);
                    table.ForeignKey(
                        name: "FK_ENROLLMENT_CATEGORY_category_id",
                        column: x => x.category_id,
                        principalTable: "CATEGORY",
                        principalColumn: "category_id");
                    table.ForeignKey(
                        name: "FK_ENROLLMENT_EVENT_event_id",
                        column: x => x.event_id,
                        principalTable: "EVENT",
                        principalColumn: "event_id");
                    table.ForeignKey(
                        name: "FK_ENROLLMENT_PARTICIPANT_participant_id",
                        column: x => x.participant_id,
                        principalTable: "PARTICIPANT",
                        principalColumn: "participant_id");
                });

            migrationBuilder.CreateTable(
                name: "RESULTS",
                columns: table => new
                {
                    results_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrolment_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    finish_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RESULTS", x => x.results_id);
                    table.ForeignKey(
                        name: "FK_RESULTS_ENROLLMENT_enrolment_id",
                        column: x => x.enrolment_id,
                        principalTable: "ENROLLMENT",
                        principalColumn: "enrolment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CATEGORY_event_id_category_name",
                table: "CATEGORY",
                columns: new[] { "event_id", "category_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ENROLLMENT_category_id",
                table: "ENROLLMENT",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_ENROLLMENT_event_id",
                table: "ENROLLMENT",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_ENROLLMENT_participant_id_event_id",
                table: "ENROLLMENT",
                columns: new[] { "participant_id", "event_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EVENT_organiser_id",
                table: "EVENT",
                column: "organiser_id");

            migrationBuilder.CreateIndex(
                name: "IX_ORGANISER_profile_id",
                table: "ORGANISER",
                column: "profile_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PARTICIPANT_profile_id",
                table: "PARTICIPANT",
                column: "profile_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PROFILE_user_id",
                table: "PROFILE",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RESULTS_enrolment_id",
                table: "RESULTS",
                column: "enrolment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROUTE_event_id",
                table: "ROUTE",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_email",
                table: "USER",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_username",
                table: "USER",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RESULTS");

            migrationBuilder.DropTable(
                name: "ROUTE");

            migrationBuilder.DropTable(
                name: "ENROLLMENT");

            migrationBuilder.DropTable(
                name: "CATEGORY");

            migrationBuilder.DropTable(
                name: "PARTICIPANT");

            migrationBuilder.DropTable(
                name: "EVENT");

            migrationBuilder.DropTable(
                name: "ORGANISER");

            migrationBuilder.DropTable(
                name: "PROFILE");

            migrationBuilder.DropTable(
                name: "USER");
        }
    }
}
