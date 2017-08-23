namespace RZUEP_CA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dodinfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Zajecias", "info", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Zajecias", "info");
        }
    }
}
