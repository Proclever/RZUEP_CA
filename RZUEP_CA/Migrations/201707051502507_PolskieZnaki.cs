namespace RZUEP_CA.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PolskieZnaki : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plans", "wydzial", c => c.String());
            DropColumn("dbo.Plans", "wydział");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Plans", "wydział", c => c.String());
            DropColumn("dbo.Plans", "wydzial");
        }
    }
}
