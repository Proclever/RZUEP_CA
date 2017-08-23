namespace RZUEP_CA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dzien : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Zajecias", "dzien", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Zajecias", "dzien");
        }
    }
}
