namespace Nerd.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InformationArchitecture000 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CompetitionEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                        CompetitionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Competitions", t => t.CompetitionId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.CompetitionId);
            
            CreateTable(
                "dbo.Competitions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedDate = c.DateTime(nullable: false),
                        Description = c.String(),
                        UserId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Location = c.String(),
                        EventType = c.Int(nullable: false),
                        Tags = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Users", "Tags", c => c.String());
            AddColumn("dbo.Users", "Interests", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Events", "UserId", "dbo.Users");
            DropForeignKey("dbo.CompetitionEntries", "UserId", "dbo.Users");
            DropForeignKey("dbo.CompetitionEntries", "CompetitionId", "dbo.Competitions");
            DropForeignKey("dbo.Competitions", "UserId", "dbo.Users");
            DropIndex("dbo.Events", new[] { "UserId" });
            DropIndex("dbo.Competitions", new[] { "UserId" });
            DropIndex("dbo.CompetitionEntries", new[] { "CompetitionId" });
            DropIndex("dbo.CompetitionEntries", new[] { "UserId" });
            DropColumn("dbo.Users", "Interests");
            DropColumn("dbo.Users", "Tags");
            DropTable("dbo.Events");
            DropTable("dbo.Competitions");
            DropTable("dbo.CompetitionEntries");
        }
    }
}
