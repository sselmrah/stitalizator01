namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Period_isMetaPeriod : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Periods", "IsMetaPeriod", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Periods", "IsMetaPeriod");
        }
    }
}
