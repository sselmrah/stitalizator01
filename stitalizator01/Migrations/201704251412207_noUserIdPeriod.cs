namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class noUserIdPeriod : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Periods", "UserID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Periods", "UserID", c => c.Int(nullable: false));
        }
    }
}
