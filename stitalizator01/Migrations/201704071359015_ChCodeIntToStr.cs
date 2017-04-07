namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChCodeIntToStr : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Programs", "ChannelCode", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Programs", "ChannelCode", c => c.Int(nullable: false));
        }
    }
}
