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
            Role admin = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "admin",
                Rank = "Глава гильдии",
                AccessLevel = 5
            };

            Role officer = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "elite",
                Rank = "Элита",
                AccessLevel = 4
            };

            Role member = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "member",
                Rank = "Член гильдии",
                AccessLevel = 3
            };

            Role recruit = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "recruit",
                Rank = "Рекрут",
                AccessLevel = 2
            };

            Role user = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "user",
                Rank = "Мимо крокодил",
                AccessLevel = 1
            };
            #endregion

            #region Добавление игровых классов
            CharacterClass paladin = new CharacterClass()
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_paladin.jpg",
                ClassName = "Паладин",
                ClassColor = "#f58cba"
            };
            CharacterClass warrior = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_warrior.jpg",
                ClassName = "Воин",
                ClassColor = "#c79c6e"
            };
            CharacterClass hunter = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_hunter.jpg",
                ClassName = "Охотник",
                ClassColor = "#abd473"
            };
            CharacterClass shaman = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_shaman.jpg",
                ClassName = "Шаман",
                ClassColor = "#0070de"
            };
            CharacterClass rogue = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_rogue.jpg",
                ClassName = "Разбойник",
                ClassColor = "#fff569"
            };
            CharacterClass druid = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_druid.jpg",
                ClassName = "Друид",
                ClassColor = "#ff7d0a"
            };
            CharacterClass priest = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_priest.jpg",
                ClassName = "Жрец",
                ClassColor = "#ffffff"
            };
            CharacterClass mage = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_mage.jpg",
                ClassName = "Маг",
                ClassColor = "#69ccf0"
            };
            CharacterClass warlock = new CharacterClass
            {
                Id = Guid.NewGuid(),
                ClassIconPath = "/images/class_icon_warlock.jpg",
                ClassName = "Чернокнижник",
                ClassColor = "#9482c9"
            };
            #endregion

            #region Добавление пользователей
            User Administrator = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Administrator",
                Email = "admin@lol.ru",
                Password = "123456".HashString(),
                RegisterDate = DateTime.Now,
                RoleId = admin.Id,
                CharacterClassId = paladin.Id
            };
            #endregion

            modelBuilder.Entity<CharacterClass>().HasData( new CharacterClass[] { paladin, warrior, shaman, hunter, rogue, druid, priest, mage, warlock } );
            modelBuilder.Entity<Role>().HasData(new Role[] { admin, officer, member, recruit, user });
            modelBuilder.Entity<User>().HasData(new User[] { Administrator });
            base.OnModelCreating(modelBuilder);
        }
    }
}
