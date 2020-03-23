namespace document.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initiatedatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.documents",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PIN = c.String(),
                        FileName = c.String(),
                        Directory = c.String(),
                        FilePassword = c.String(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Pin = c.String(),
                        Action = c.String(),
                        Location = c.String(),
                        OriginalLocation = c.String(),
                        Created = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Logs");
            DropTable("dbo.documents");
        }
    }
}
