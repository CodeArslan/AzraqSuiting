namespace AzraqSuiting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeBarcodePropertyType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "BarCode", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "BarCode", c => c.Long(nullable: false));
        }
    }
}
