namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChannelsAdded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Channels",
                c => new
                    {
                        ChannelID = c.Int(nullable: false, identity: true),
                        ChannelCode = c.Int(nullable: false),
                        ChannelTag = c.String(),
                        ChannelName = c.String(),
                    })
                .PrimaryKey(t => t.ChannelID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Channels");
        }
    }
}
