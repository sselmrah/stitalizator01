namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Period_score : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Periods", "ScoresGambled", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Periods", "ScoresGambled");
        }
    }
}
