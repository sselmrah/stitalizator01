namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TelegramChatId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "TelegramChatId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "TelegramChatId");
        }
    }
}
