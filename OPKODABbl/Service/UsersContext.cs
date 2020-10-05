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
        public DbSet<CharacterClass> CharacterClasses { get; set; }
        public DbSet<AvatarImage> AvatarImages { get; set; }

        public UsersContext(DbContextOptions<UsersContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Добавление ролей
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
            #endregion

            #region Добавление игровых классов
            List<CharacterClass> characterClasses = new List<CharacterClass>()
            {
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "",
                    ClassName = "-Выберите класс персонажа-"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_paladin.jpg",
                    ClassName = "Паладин"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_warrior.jpg",
                    ClassName = "Воин"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_hunter.jpg",
                    ClassName = "Охотник"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_shaman.jpg",
                    ClassName = "Шаман"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_rogue.jpg",
                    ClassName = "Разбойник"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_druid.jpg",
                    ClassName = "Друид"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_priest.jpg",
                    ClassName = "Жрец"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_mage.jpg",
                    ClassName = "Маг"
                },
                new CharacterClass
                {
                    Id = Guid.NewGuid(),
                    ClassIconPath = "/images/class_icon_warlock.jpg",
                    ClassName = "Чернокнижник"
                }
            };
            #endregion

            modelBuilder.Entity<CharacterClass>().HasData( characterClasses );
            modelBuilder.Entity<Role>().HasData(new Role[] { adminRole, clanMember, clanRecruit, userRole });
            modelBuilder.Entity<User>().HasData(new User[] { Administrator });
            base.OnModelCreating(modelBuilder);
        }
    }
}
