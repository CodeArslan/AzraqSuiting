namespace AzraqSuiting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBrandModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Brands",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Products", "BrandId", c => c.Int());
            AddColumn("dbo.Products", "CurrentValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "AverageCost", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Products", "MinimumStock", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Products", "Name", c => c.String(nullable: false));
            CreateIndex("dbo.Products", "BrandId");
            AddForeignKey("dbo.Products", "BrandId", "dbo.Brands", "Id");
            DropColumn("dbo.Products", "Unit");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "Unit", c => c.String());
            DropForeignKey("dbo.Products", "BrandId", "dbo.Brands");
            DropIndex("dbo.Products", new[] { "BrandId" });
            AlterColumn("dbo.Products", "Name", c => c.String());
            DropColumn("dbo.Products", "MinimumStock");
            DropColumn("dbo.Products", "AverageCost");
            DropColumn("dbo.Products", "CurrentValue");
            DropColumn("dbo.Products", "BrandId");
            DropTable("dbo.Brands");
        }
    }
}
