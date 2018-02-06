namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class catEntriesAppDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CatalogueEntries",
                c => new
                    {
                        CatalogueEntryID = c.Int(nullable: false, identity: true),
                        Timing = c.DateTime(nullable: false),
                        Title = c.String(),
                        Repeat = c.Boolean(nullable: false),
                        TVDate = c.DateTime(nullable: false),
                        Dow = c.Int(nullable: false),
                        BegTime = c.DateTime(nullable: false),
                        Sti = c.Single(nullable: false),
                        Dm = c.Single(nullable: false),
                        Dr = c.Single(nullable: false),
                        ProducerCode = c.Int(nullable: false),
                        SellerCode = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CatalogueEntryID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CatalogueEntries");
        }
    }
}
