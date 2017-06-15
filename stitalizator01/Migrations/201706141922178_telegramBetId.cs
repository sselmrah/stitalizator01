namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class telegramBetId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "TelegramBetId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "TelegramBetId");
        }
    }
}
