namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tgbot2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConversationStarters", "LastTimeUsed", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConversationStarters", "LastTimeUsed");
        }
    }
}
