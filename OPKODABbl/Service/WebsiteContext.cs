using Microsoft.EntityFrameworkCore;
using OPKODABbl.Helpers;
using OPKODABbl.Models.Account;
using OPKODABbl.Models.Gallery;
using OPKODABbl.Models.Main;
using OPKODABbl.Models.Forum;
using System;
using System.Linq;

namespace OPKODABbl.Service
{
    public class WebsiteContext : DbContext
    {
        public DbSet<News> News { get; set; }
        public DbSet<NewsImage> NewsImages { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<CharacterClass> CharacterClasses { get; set; }
        public DbSet<AvatarImage> AvatarImages { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Subsection> Subsections { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Reply> Replies { get; set; }

        public WebsiteContext(DbContextOptions<WebsiteContext> options) : base(options)
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
                Color = "#ff0000",
                AccessLevel = 5
            };

            Role officer = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "elite",
                Rank = "Элита",
                Color = "#00ff00",
                AccessLevel = 4
            };

            Role member = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "member",
                Rank = "Боец",
                Color = "#cdcdcd",
                AccessLevel = 3
            };

            Role recruit = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "recruit",
                Rank = "Рекрут",
                Color = "#828282",
                AccessLevel = 2
            };

            Role user = new Role()
            {
                Id = Guid.NewGuid(),
                Name = "user",
                Rank = "Мимо крокодил",
                Color = "#000000",
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
                IsConfirmed = true,
                RoleId = admin.Id,
                CharacterClassId = paladin.Id
            };

            User Anonymous = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Anonymous",
                IsConfirmed = true,
                RoleId = user.Id,
                CharacterClassId = paladin.Id
            };

            User user1 = new User()
            {
                Id = Guid.NewGuid(),
                Name = "User1",
                Email = "user1@lol.ru",
                Password = "123456".HashString(),
                RegisterDate = DateTime.Now,
                IsConfirmed = true,
                RoleId = user.Id,
                CharacterClassId = rogue.Id
            };

            User user2 = new User()
            {
                Id = Guid.NewGuid(),
                Name = "User2",
                Email = "user2@lol.ru",
                Password = "123456".HashString(),
                RegisterDate = DateTime.Now,
                IsConfirmed = true,
                RoleId = user.Id,
                CharacterClassId = priest.Id
            };

            User user3 = new User()
            {
                Id = Guid.NewGuid(),
                Name = "User3",
                Email = "user3@lol.ru",
                Password = "123456".HashString(),
                RegisterDate = DateTime.Now,
                IsConfirmed = true,
                RoleId = user.Id,
                CharacterClassId = shaman.Id
            };
            #endregion

            modelBuilder.Entity<CharacterClass>().HasData(new CharacterClass[] { paladin, warrior, shaman, hunter, rogue, druid, priest, mage, warlock });
            modelBuilder.Entity<Role>().HasData(new Role[] { admin, officer, member, recruit, user });
            modelBuilder.Entity<User>().HasData(new User[] { Administrator, Anonymous, user1, user2, user3 });

            base.OnModelCreating(modelBuilder);
        }
    }
}
