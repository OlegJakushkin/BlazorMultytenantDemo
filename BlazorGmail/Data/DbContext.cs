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
            Database.EnsureCreated();
        }

        #endregion

        #region Overidden methods

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Org>().HasData(GetOrgs());
            modelBuilder.Entity<User>().HasData(GetUsers());

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


        #region Private methods

        private List<Org> GetOrgs()
        {
            return new List<Org>
            {
                new Org {Id = 1001, AdminName = "MS"},
                new Org {Id = 1002, AdminName = "SPBU"},
                new Org {Id = 1003, AdminName = "APPL"},
                new Org {Id = 1004, AdminName = "GOOGL"}
            };
        }

        private List<User> GetUsers()
        {
            var ul = new List<User>
            {
                new User {Id = 1001, Name = "Vasia"},
                new User {Id = 1002, Name = "Dasha"},
                new User {Id = 1003, Name = "Petia"},
                new User {Id = 1004, Name = "Masha"}
            };


            //ul.First().Orgs.Add(new Org { Id = 1005, AdminName = "AMD" });

            return ul;
        }

        #endregion
    }
}