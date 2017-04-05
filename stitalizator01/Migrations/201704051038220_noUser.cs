namespace stitalizator01.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class noUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bets",
                c => new
                    {
                        BetID = c.Int(nullable: false, identity: true),
                        PeriodID = c.Int(nullable: false),
                        ScoreOLS = c.Single(nullable: false),
                        ScoreClassic = c.Single(nullable: false),
                        AttemptNo = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                        IsHorse = c.Boolean(nullable: false),
                        BetRus18 = c.Single(nullable: false),
                        BetMos18 = c.Single(nullable: false),
                        BetSTImob = c.Single(nullable: false),
                        BetSTI = c.Single(nullable: false),
                        BetSTIplus = c.Single(nullable: false),
                        ProgramID = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.BetID)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .ForeignKey("dbo.Programs", t => t.ProgramID, cascadeDelete: true)
                .Index(t => t.ProgramID)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Periods",
                c => new
                    {
                        PeriodID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        PeriodDescription = c.String(),
                        EndDate = c.DateTime(nullable: false),
                        BegDate = c.DateTime(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        Bet_BetID = c.Int(),
                    })
                .PrimaryKey(t => t.PeriodID)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .ForeignKey("dbo.Bets", t => t.Bet_BetID)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Bet_BetID);
            
            CreateTable(
                "dbo.Programs",
                c => new
                    {
                        ProgramID = c.Int(nullable: false, identity: true),
                        ChannelCode = c.Int(nullable: false),
                        ProgTitle = c.String(),
                        TvDate = c.DateTime(nullable: false),
                        TimeStart = c.DateTime(nullable: false),
                        TimeEnd = c.DateTime(),
                        ProgDescr = c.String(),
                        ShareStiPlus = c.Single(),
                        ShareStiMob = c.Single(),
                        ShareSti = c.Single(),
                        ShareMos18 = c.Single(),
                        ShareRus18 = c.Single(),
                    })
                .PrimaryKey(t => t.ProgramID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Bets", "ProgramID", "dbo.Programs");
            DropForeignKey("dbo.Periods", "Bet_BetID", "dbo.Bets");
            DropForeignKey("dbo.Periods", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Bets", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Periods", new[] { "Bet_BetID" });
            DropIndex("dbo.Periods", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Bets", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.Bets", new[] { "ProgramID" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Programs");
            DropTable("dbo.Periods");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Bets");
        }
    }
}
