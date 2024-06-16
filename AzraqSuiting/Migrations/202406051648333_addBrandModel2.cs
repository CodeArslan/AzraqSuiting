namespace AzraqSuiting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBrandModel2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "Unit", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "Unit");
        }
    }
}
