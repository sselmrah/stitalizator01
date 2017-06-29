namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConversationStarters",
                c => new
                    {
                        ConversationStarterID = c.Int(nullable: false, identity: true),
                        ToId = c.String(),
                        ToName = c.String(),
                        FromId = c.String(),
                        FromName = c.String(),
                        ServiceUrl = c.String(),
                        ChannelId = c.String(),
                        ConversationId = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ConversationStarterID)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ConversationStarters", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ConversationStarters", new[] { "ApplicationUser_Id" });
            DropTable("dbo.ConversationStarters");
        }
    }
}
