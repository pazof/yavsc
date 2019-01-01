using Microsoft.Data.Entity.Migrations;

namespace Yavsc.Migrations
{
    public partial class paypalToDeprecated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable( name: "PaypalPayment", newName: "PayPalPayment");
            migrationBuilder.RenameColumn ( name:"PaypalPaymentId", table: "PayPalPayment", newName:"CreationToken" );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn ( newName :"PaypalPaymentId", table: "PayPalPayment", name:"CreationToken" );
            migrationBuilder.RenameTable( newName: "PaypalPayment", name: "PayPalPayment");
        }
    }
}
