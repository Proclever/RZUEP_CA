namespace RZUEP_CA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Pro : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Grupies",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        prozajeciaid = c.Int(nullable: false),
                        nazwa = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Prozajecias", t => t.prozajeciaid, cascadeDelete: true)
                .Index(t => t.prozajeciaid);
            
            CreateTable(
                "dbo.Prozajecias",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        proprowadzacyid = c.Int(nullable: false),
                        dzien = c.Int(nullable: false),
                        godzinaod = c.String(),
                        godzinado = c.String(),
                        rodzaj = c.String(),
                        nazwa = c.String(),
                        sala = c.String(),
                        info = c.String(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Proprowadzacies", t => t.proprowadzacyid, cascadeDelete: true)
                .Index(t => t.proprowadzacyid);
            
            CreateTable(
                "dbo.Proprowadzacies",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        semestr = c.String(),
                        tryb = c.String(),
                        wydzial = c.String(),
                        katedra = c.String(),
                        nazwa = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Grupies", "prozajeciaid", "dbo.Prozajecias");
            DropForeignKey("dbo.Prozajecias", "proprowadzacyid", "dbo.Proprowadzacies");
            DropIndex("dbo.Prozajecias", new[] { "proprowadzacyid" });
            DropIndex("dbo.Grupies", new[] { "prozajeciaid" });
            DropTable("dbo.Proprowadzacies");
            DropTable("dbo.Prozajecias");
            DropTable("dbo.Grupies");
        }
    }
}
