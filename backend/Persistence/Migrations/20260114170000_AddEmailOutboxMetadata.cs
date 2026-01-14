using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdSPMdS.DemanioDigitale.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailOutboxMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentsJson",
                schema: "messaging",
                table: "EmailOutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BccJson",
                schema: "messaging",
                table: "EmailOutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CcJson",
                schema: "messaging",
                table: "EmailOutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HtmlBody",
                schema: "messaging",
                table: "EmailOutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplyTo",
                schema: "messaging",
                table: "EmailOutboxMessages",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentsJson",
                schema: "messaging",
                table: "EmailOutboxMessages");

            migrationBuilder.DropColumn(
                name: "BccJson",
                schema: "messaging",
                table: "EmailOutboxMessages");

            migrationBuilder.DropColumn(
                name: "CcJson",
                schema: "messaging",
                table: "EmailOutboxMessages");

            migrationBuilder.DropColumn(
                name: "HtmlBody",
                schema: "messaging",
                table: "EmailOutboxMessages");

            migrationBuilder.DropColumn(
                name: "ReplyTo",
                schema: "messaging",
                table: "EmailOutboxMessages");
        }
    }
}
