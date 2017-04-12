namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bet_isLocked : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bets", "IsLocked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Bets", "IsLocked");
        }
    }
}
