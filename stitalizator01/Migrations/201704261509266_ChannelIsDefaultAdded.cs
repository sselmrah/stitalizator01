namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChannelIsDefaultAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Channels", "IsDefault", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Channels", "IsDefault");
        }
    }
}
