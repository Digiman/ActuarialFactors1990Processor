using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataProcessingApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblB",
                columns: table => new
                {
                    Years = table.Column<double>(type: "float", nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false),
                    pvAnnuity = table.Column<double>(type: "float", nullable: false),
                    pvIncomeInterest = table.Column<double>(type: "float", nullable: false),
                    pvRemainderInterest = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblC",
                columns: table => new
                {
                    MortalityTable = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<double>(type: "float", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    remainderFactor = table.Column<double>(type: "float", nullable: false),
                    rFactor = table.Column<double>(type: "float", nullable: false),
                    dFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblD",
                columns: table => new
                {
                    Years = table.Column<int>(type: "int", nullable: false),
                    PayoutRate = table.Column<double>(type: "float", nullable: false),
                    remainderInterest = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblF",
                columns: table => new
                {
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Months = table.Column<int>(type: "int", nullable: false),
                    adjustmentFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblH",
                columns: table => new
                {
                    MortalityTable = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    dFactor = table.Column<double>(type: "float", nullable: false),
                    nFactor = table.Column<double>(type: "float", nullable: false),
                    mFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblJ",
                columns: table => new
                {
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    adjustmentFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblK",
                columns: table => new
                {
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    adjustmentFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblMortality",
                columns: table => new
                {
                    Year = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    lx = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblR2",
                columns: table => new
                {
                    MortalityTable = table.Column<int>(type: "int", nullable: false),
                    Age1 = table.Column<int>(type: "int", nullable: false),
                    Age2 = table.Column<int>(type: "int", nullable: false),
                    AdjustedPayoutRate = table.Column<double>(type: "float", nullable: false),
                    remainderFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblS",
                columns: table => new
                {
                    MortalityTable = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    pvAnnuity = table.Column<double>(type: "float", nullable: false),
                    pvLifeEstate = table.Column<double>(type: "float", nullable: false),
                    pvRemainderInterest = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblU1",
                columns: table => new
                {
                    MortalityTable = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    AdjustedPayoutRate = table.Column<double>(type: "float", nullable: false),
                    remainderFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblU2",
                columns: table => new
                {
                    MortalityTable = table.Column<int>(type: "int", nullable: false),
                    Age1 = table.Column<int>(type: "int", nullable: false),
                    Age2 = table.Column<int>(type: "int", nullable: false),
                    AdjustedPayoutRate = table.Column<double>(type: "float", nullable: false),
                    remainderFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "tblZ",
                columns: table => new
                {
                    MortalityTable = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<double>(type: "float", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    dFactor = table.Column<double>(type: "float", nullable: false),
                    nFactor = table.Column<double>(type: "float", nullable: false),
                    mFactor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblB");

            migrationBuilder.DropTable(
                name: "tblC");

            migrationBuilder.DropTable(
                name: "tblD");

            migrationBuilder.DropTable(
                name: "tblF");

            migrationBuilder.DropTable(
                name: "tblH");

            migrationBuilder.DropTable(
                name: "tblJ");

            migrationBuilder.DropTable(
                name: "tblK");

            migrationBuilder.DropTable(
                name: "tblMortality");

            migrationBuilder.DropTable(
                name: "tblR2");

            migrationBuilder.DropTable(
                name: "tblS");

            migrationBuilder.DropTable(
                name: "tblU1");

            migrationBuilder.DropTable(
                name: "tblU2");

            migrationBuilder.DropTable(
                name: "tblZ");
        }
    }
}
