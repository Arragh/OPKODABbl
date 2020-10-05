using Microsoft.EntityFrameworkCore;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPKODABbl.Service
{
    public class UsersContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AvatarImage> AvatarImages { get; set; }

        public UsersContext(DbContextOptions<UsersContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // добавляем роли
            Role adminRole = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "admin"
            };

            Role clanMember = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "clanmember"
            };

            Role clanRecruit = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "recruit"
            };

            Role userRole = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "user"
            };

            User Administrator = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Administrator",
                Email = "admin@lol.ru",
                Password = "123456".HashString(),
                RoleId = adminRole.Id
            };

            modelBuilder.Entity<Role>().HasData(new Role[] { adminRole, clanMember, clanRecruit, userRole });
            modelBuilder.Entity<User>().HasData(new User[] { Administrator });
            base.OnModelCreating(modelBuilder);
        }
    }
}
