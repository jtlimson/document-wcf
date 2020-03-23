namespace document.Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ContextDB : DbContext
    {
        public ContextDB(string connection)
             : base(connection)
        //-- NOTE: Replace the context above with hardcoded context (code below) when trying to update migration --e.g. Update-Database .  --
        //public ContextDB() 
        //      : base("name=ContextDB")
        {
        }

        public virtual DbSet<Efiling> Efiling { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }
    }
}
