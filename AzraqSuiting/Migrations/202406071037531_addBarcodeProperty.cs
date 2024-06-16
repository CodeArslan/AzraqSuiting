namespace AzraqSuiting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBarcodeProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "BarcodeImagePath", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "BarcodeImagePath");
        }
    }
}
