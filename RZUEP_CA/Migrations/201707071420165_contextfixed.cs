namespace RZUEP_CA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class contextfixed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Proprowadzacies", "jednostka", c => c.String());
            DropColumn("dbo.Proprowadzacies", "katedra");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Proprowadzacies", "katedra", c => c.String());
            DropColumn("dbo.Proprowadzacies", "jednostka");
        }
    }
}
