namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class betToDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Programs", "ShareStiPlus", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Programs", "ShareStiPlus", c => c.Single());
        }
    }
}
