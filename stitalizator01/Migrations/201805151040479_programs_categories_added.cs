namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class programs_categories_added : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Programs", "Cat1_Id", c => c.Int());
            AddColumn("dbo.Programs", "Cat2_Id", c => c.Int());
            CreateIndex("dbo.Programs", "Cat1_Id");
            CreateIndex("dbo.Programs", "Cat2_Id");
            AddForeignKey("dbo.Programs", "Cat1_Id", "dbo.Categories", "Id");
            AddForeignKey("dbo.Programs", "Cat2_Id", "dbo.Categories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Programs", "Cat2_Id", "dbo.Categories");
            DropForeignKey("dbo.Programs", "Cat1_Id", "dbo.Categories");
            DropIndex("dbo.Programs", new[] { "Cat2_Id" });
            DropIndex("dbo.Programs", new[] { "Cat1_Id" });
            DropColumn("dbo.Programs", "Cat2_Id");
            DropColumn("dbo.Programs", "Cat1_Id");
        }
    }
}
