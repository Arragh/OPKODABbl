using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OPKODABbl.Models.Forum;
using OPKODABbl.Service;

namespace OPKODABbl.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ForumController : Controller
    {
        private readonly WebsiteContext _websiteDB;

        public ForumController(WebsiteContext websiteContext)
        {
            _websiteDB = websiteContext;
        }

        #region Список разделов и подразделов
        public async Task<IActionResult> Sections()
        {
            List<Section> sections = await _websiteDB.Sections.Include(f => f.Subsections).ToListAsync();

            return View(sections);
        }
        #endregion

        #region Создание раздела
        public async Task<IActionResult> CreateSection(string sectionName)
        {
            if (!string.IsNullOrWhiteSpace(sectionName))
            {
                int position = await _websiteDB.Sections.CountAsync() + 1;

                Section section = new Section()
                {
                    Id = Guid.NewGuid(),
                    SectionName = sectionName,
                    SectionPosition = position
                };

                await _websiteDB.Sections.AddAsync(section);
                await _websiteDB.SaveChangesAsync();

                return RedirectToAction("Sections", "Forum");
            }

            return RedirectToAction("Sections", "Forum");
        }
        #endregion

        #region Создание подраздела
        public async Task<IActionResult> CreateSubsection(Guid sectionId, string subsectionName)
        {
            Section section = await _websiteDB.Sections.FirstOrDefaultAsync(s => s.Id == sectionId);

            int position = await _websiteDB.Subsections.Where(s => s.SectionId == sectionId).CountAsync() + 1;

            if (section != null && !string.IsNullOrWhiteSpace(subsectionName))
            {
                Subsection subsection = new Subsection()
                {
                    Id = Guid.NewGuid(),
                    SubsectionName = subsectionName,
                    SectionId = section.Id,
                    SubsectionPosition = position
                };

                await _websiteDB.Subsections.AddAsync(subsection);
                await _websiteDB.SaveChangesAsync();

                return RedirectToAction("Sections", "Forum");
            }

            return RedirectToAction("Sections", "Forum");
        }
        #endregion

        #region Сдвиг раздела вверх
        public async Task<IActionResult> MoveSectionUp(Guid sectionId)
        {
            List<Section> sections = await _websiteDB.Sections.OrderBy(s => s.SectionPosition).ToListAsync();
            Section section = sections.FirstOrDefault(s => s.Id == sectionId);

            if (section != null && section.SectionPosition > 1)
            {
                // Создаем цикл перебора для прохождения по всему списку
                for (int i = 0; i < sections.Count; i++)
                {
                    // Находим нужный нам раздел по Id
                    if (sections[i].Id == sectionId)
                    {
                        // Меняем местами значение позиций у нужного нам раздела и того, что стоит на 1 ступень выше
                        int temp = sections[i].SectionPosition;
                        sections[i].SectionPosition = sections[i - 1].SectionPosition;
                        sections[i - 1].SectionPosition = temp;
                    }
                }

                _websiteDB.Sections.Update(section);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("Sections", "Forum");
        }
        #endregion

        #region Сдвиг раздела вниз
        public async Task<IActionResult> MoveSectionDown(Guid sectionId)
        {
            List<Section> sections = await _websiteDB.Sections.OrderBy(s => s.SectionPosition).ToListAsync();
            Section section = sections.FirstOrDefault(s => s.Id == sectionId);

            int maxPosition = sections.Count();

            if (section != null && section.SectionPosition < maxPosition)
            {
                // Создаем цикл перебора для прохождения по всему списку
                for (int i = 0; i < sections.Count; i++)
                {
                    // Находим нужный нам раздел по Id
                    if (sections[i].Id == sectionId)
                    {
                        // Меняем местами значение позиций у нужного нам раздела и того, что стоит на 1 ступень ниже
                        int temp = sections[i].SectionPosition;
                        sections[i].SectionPosition = sections[i + 1].SectionPosition;
                        sections[i + 1].SectionPosition = temp;
                    }
                }

                _websiteDB.Sections.Update(section);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("Sections", "Forum");
        }
        #endregion

        #region Сдвиг подраздела вверх
        public async Task<IActionResult> MoveSubsectionUp(Guid sectionId, Guid subsectionId)
        {
            List<Subsection> subsections = await _websiteDB.Subsections.Where(s => s.SectionId == sectionId).OrderBy(s => s.SubsectionPosition).ToListAsync();
            Subsection subsection = subsections.FirstOrDefault(s => s.Id == subsectionId);

            if (subsection != null && subsection.SubsectionPosition > 1)
            {
                // Создаем цикл перебора для прохождения по всему списку
                for (int i = 0; i < subsections.Count; i++)
                {
                    // Находим нужный нам подраздел по Id
                    if (subsections[i].Id == subsectionId)
                    {
                        // Меняем местами значение позиций у нужного нам подраздела и того, что стоит на 1 ступень выше
                        int temp = subsections[i].SubsectionPosition;
                        subsections[i].SubsectionPosition = subsections[i - 1].SubsectionPosition;
                        subsections[i - 1].SubsectionPosition = temp;
                    }
                }

                _websiteDB.Subsections.Update(subsection);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("Sections", "Forum");
        }
        #endregion

        #region Сдвиг подраздела вниз
        public async Task<IActionResult> MoveSubsectionDown(Guid sectionId, Guid subsectionId)
        {
            List<Subsection> subsections = await _websiteDB.Subsections.Where(s => s.SectionId == sectionId).OrderBy(s => s.SubsectionPosition).ToListAsync();
            Subsection subsection = subsections.FirstOrDefault(s => s.Id == subsectionId);

            int maxPosition = subsections.Count();

            // Если такой подраздел существует и его порядковый номер меньше максимального значения позиции, то заходим в тело условия
            if (subsection != null && subsection.SubsectionPosition < maxPosition)
            {
                // Создаем цикл перебора для прохождения по всему списку
                for (int i = 0; i < subsections.Count; i++)
                {
                    // Находим нужный нам подраздел по Id
                    if (subsections[i].Id == subsectionId)
                    {
                        // Меняем местами значение позиций у нужного нам подраздела и того, что стоит на 1 ступень ниже
                        int temp = subsections[i].SubsectionPosition;
                        subsections[i].SubsectionPosition = subsections[i + 1].SubsectionPosition;
                        subsections[i + 1].SubsectionPosition = temp;
                    }
                }

                _websiteDB.Subsections.Update(subsection);
                await _websiteDB.SaveChangesAsync();
            }

            return RedirectToAction("Sections", "Forum");
        }
        #endregion

    }
}
