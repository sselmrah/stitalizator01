namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProgramIsHorse : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Programs", "IsHorse", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Programs", "IsHorse");
        }
    }
}
