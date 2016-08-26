namespace Nerd.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "Deleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Posts", "PostType", c => c.Int(nullable: false));
            AddColumn("dbo.Users", "AspNetUserId", c => c.String());
            AddColumn("dbo.Users", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Users", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IsActive");
            DropColumn("dbo.Users", "CreatedDate");
            DropColumn("dbo.Users", "AspNetUserId");
            DropColumn("dbo.Posts", "PostType");
            DropColumn("dbo.Comments", "Deleted");
        }
    }
}
