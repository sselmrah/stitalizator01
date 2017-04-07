namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProgCatAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Programs", "ProgCat", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Programs", "ProgCat");
        }
    }
}
