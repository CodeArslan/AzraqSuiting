namespace AzraqSuiting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDataAnotatio : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Brands", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Categories", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Categories", "Name", c => c.String());
            AlterColumn("dbo.Brands", "Name", c => c.String());
        }
    }
}
