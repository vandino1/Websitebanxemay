using Microsoft.EntityFrameworkCore.Migrations;

namespace websitebanxemay.Data.Migrations
{
    public partial class add_thongke : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Desciption",
                table: "Product",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    StatisticsID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalOrders = table.Column<int>(nullable: false),
                    TotalCustomers = table.Column<int>(nullable: false),
                    TotalRevenue = table.Column<int>(nullable: false),
                    BillDetailId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.StatisticsID);
                    table.ForeignKey(
                        name: "FK_Statistics_BillDetail_BillDetailId",
                        column: x => x.BillDetailId,
                        principalTable: "BillDetail",
                        principalColumn: "BillDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_BillDetailId",
                table: "Statistics",
                column: "BillDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.AlterColumn<string>(
                name: "Desciption",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
