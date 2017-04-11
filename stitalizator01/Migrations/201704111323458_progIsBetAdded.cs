namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class progIsBetAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Programs", "IsBet", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Programs", "IsBet");
        }
    }
}
