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
            #endregion

            #region Добавление игровых классов
            CharacterClass paladin = new CharacterClass()
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_paladin.jpg",
                ClassName = "Паладин"
            };
            CharacterClass warrior = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_warrior.jpg",
                ClassName = "Воин"
            };
            CharacterClass hunter = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_hunter.jpg",
                ClassName = "Охотник"
            };
            CharacterClass shaman = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_shaman.jpg",
                ClassName = "Шаман"
            };
            CharacterClass rogue = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_rogue.jpg",
                ClassName = "Разбойник"
            };
            CharacterClass druid = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_druid.jpg",
                ClassName = "Друид"
            };
            CharacterClass priest = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_priest.jpg",
                ClassName = "Жрец"
            };
            CharacterClass mage = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_mage.jpg",
                ClassName = "Маг"
            };
            CharacterClass warlock = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_warlock.jpg",
                ClassName = "Чернокнижник"
            };
            #endregion

            #region Добавление пользователей
            User Administrator = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Administrator",
                Email = "admin@lol.ru",
                Password = "123456".HashString(),
                RoleId = adminRole.Id,
                CharacterClassId = paladin.Id
            };
            #endregion

            modelBuilder.Entity<CharacterClass>().HasData( new CharacterClass[] { paladin, warrior, shaman, hunter, rogue, druid, priest, mage, warlock } );
            modelBuilder.Entity<Role>().HasData(new Role[] { adminRole, clanMember, clanRecruit, userRole });
            modelBuilder.Entity<User>().HasData(new User[] { Administrator });
            base.OnModelCreating(modelBuilder);
        }
    }
}
