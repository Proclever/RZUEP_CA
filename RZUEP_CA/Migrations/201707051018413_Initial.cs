namespace RZUEP_CA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Plans",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        semestr = c.String(),
                        tryb = c.String(),
                        stopien = c.String(),
                        wydział = c.String(),
                        rok = c.String(),
                        kierunek = c.String(),
                        grupa = c.String(),
                        specjalnosc = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Zajecias",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        planid = c.Int(nullable: false),
                        godzinaod = c.String(),
                        godzinado = c.String(),
                        rodzaj = c.String(),
                        nazwa = c.String(),
                        sala = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Plans", t => t.planid, cascadeDelete: true)
                .Index(t => t.planid);
            
            CreateTable(
                "dbo.Prowadzacies",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        zajeciaid = c.Int(nullable: false),
                        nazwa = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Zajecias", t => t.zajeciaid, cascadeDelete: true)
                .Index(t => t.zajeciaid);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Prowadzacies", "zajeciaid", "dbo.Zajecias");
            DropForeignKey("dbo.Zajecias", "planid", "dbo.Plans");
            DropIndex("dbo.Prowadzacies", new[] { "zajeciaid" });
            DropIndex("dbo.Zajecias", new[] { "planid" });
            DropTable("dbo.Prowadzacies");
            DropTable("dbo.Zajecias");
            DropTable("dbo.Plans");
        }
    }
}
