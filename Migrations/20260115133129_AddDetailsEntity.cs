using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentVerificationSystemApi.Migrations
{
	/// <inheritdoc />
	public partial class AddDetailsEntity : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Details",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					UploadFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					TaxpayerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					CompanyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
					TcIdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
					TaxOffice = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ActivityCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
					GrossIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
					TaxBase = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
					CalculatedTax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
					AccruedTax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
					TaxPeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ExtractedConfidence = table.Column<float>(type: "real", nullable: true),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
					TanetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Details", x => x.Id);
					table.ForeignKey(
						name: "FK_Details_Tanets_TanetId",
						column: x => x.TanetId,
						principalTable: "Tanets",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_Details_UploadFiles_UploadFileId",
						column: x => x.UploadFileId,
						principalTable: "UploadFiles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Details_TanetId",
				table: "Details",
				column: "TanetId");

			migrationBuilder.CreateIndex(
				name: "IX_Details_UploadFileId",
				table: "Details",
				column: "UploadFileId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Details");
		}
	}
}
