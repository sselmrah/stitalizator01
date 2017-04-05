using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace stitalizator01.Models
{
    public class StitalizatorContext : DbContext
    {
        public StitalizatorContext() : base("BetsDB")
        {

        }
        public DbSet<Program> Programs { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Period> Periods { get; set;}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //base.OnModelCreating(modelBuilder);
        }
    }
}