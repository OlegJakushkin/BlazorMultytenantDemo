using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlazorMultytenantDemo.Data
{

    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        #region Contructor


        public DbContext(DbContextOptions<DbContext> options)
            : base(options)
        {
            FirsRun  = Database.EnsureCreated();
        }
        public bool FirsRun { get; set; }
        
        #endregion

        #region Overidden methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>().HasData(GetUsers());

            modelBuilder.Entity<Relation>().HasKey(rt => new { rt.UserId, rt.OrgId });
            modelBuilder.Entity<Relation>()
                .HasOne(rt => rt.User)
                .WithMany(r => r.Relations)
                .HasForeignKey(rt => rt.OrgId).IsRequired();
            modelBuilder.Entity<Relation>()
                .HasOne(rt => rt.Org)
                .WithMany(r => r.Relations)
                .HasForeignKey(rt => rt.UserId).IsRequired();

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region Public properties

        public DbSet<Org> Orgs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Relation> Relations { get; set; }
        #endregion


    }
}