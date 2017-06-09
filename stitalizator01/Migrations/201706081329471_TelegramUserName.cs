namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TelegramUserName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "TelegramUserName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "TelegramUserName");
        }
    }
}
