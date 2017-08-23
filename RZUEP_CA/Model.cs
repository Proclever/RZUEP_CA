using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace RZUEP_CA
{
    public class Plan
    {
        [Key]
        public int id { get; set; }
        public string semestr { get; set; }
        public string tryb { get; set; }
        public string stopien { get; set; }
        public string wydzial { get; set; }
        public string rok { get; set; }
        public string kierunek { get; set; }
        public string grupa { get; set; }
        public string specjalnosc { get; set; }
        public virtual ICollection<Zajecia> Zajecia { get; set; }
    }

    public class Zajecia
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Plan")]
        public int planid { get; set; }
        public int dzien { get; set; }
        public string godzinaod { get; set; }
        public string godzinado { get; set; }
        public string rodzaj { get; set; }
        public string nazwa { get; set; }
        public string sala { get; set; }
        public string info { get; set; }
        public virtual Plan Plan { get; set; }
        public virtual ICollection<Prowadzacy> Prowadzący { get; set; }
    }

    public class Prowadzacy
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Zajecia")]
        public int zajeciaid { get; set; }
        public string nazwa { get; set; }
        public virtual Zajecia Zajecia { get; set; }
    }

    public class Proprowadzacy
    {
        [Key]
        public int id { get; set; }
        public string semestr { get; set; }
        public string tryb { get; set; }
        public string wydzial { get; set; }
        public string jednostka { get; set; }
        public string nazwa { get; set; }
        public virtual ICollection<Prozajecia> Prozajecia { get; set; }
    }

    public class Prozajecia
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Proprowadzacy")]
        public int proprowadzacyid { get; set; }
        public int dzien { get; set; }
        public string godzinaod { get; set; }
        public string godzinado { get; set; }
        public string rodzaj { get; set; }
        public string nazwa { get; set; }
        public string sala { get; set; }
        public string info { get; set; }
        public virtual Proprowadzacy Proprowadzacy { get; set; }
        public virtual ICollection<Grupy> Grupy { get; set; }
    }

    public class Grupy
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Prozajecia")]
        public int prozajeciaid { get; set; }
        public string nazwa { get; set; }
        public virtual Prozajecia Prozajecia { get; set; }
    }

    public class RZUEPDBContext : DbContext
    {
        public RZUEPDBContext()
        : base("RZUEPDBContext")
        {
            //AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Directory.GetCurrentDirectory());
        }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Zajecia> Zajecias { get; set; }
        public DbSet<Prowadzacy> Prowadzacys { get; set; }
        public DbSet<Proprowadzacy> Proprowadzacys { get; set; }
        public DbSet<Prozajecia> Prozajecias { get; set; }
        public DbSet<Grupy> Grupys { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
