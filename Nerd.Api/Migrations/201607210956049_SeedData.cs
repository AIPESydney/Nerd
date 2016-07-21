namespace Nerd.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedData : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO dbo.AspNetRoles VALUES ('1','Global Administrators')");
            Sql("INSERT INTO dbo.AspNetRoles VALUES ('2','Administrators')");
            Sql("INSERT INTO dbo.AspNetRoles VALUES ('3','Users')");
        }
        
        public override void Down()
        {
        }
    }
}
