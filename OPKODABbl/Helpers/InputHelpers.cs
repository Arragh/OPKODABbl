﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OPKODABbl.Helpers
{
    public static class InputHelpers
    {
        #region Удаление выбранных тегов
        public static string DeleteTags(this string text)
        {
            return text.Replace("<br>", " ")
                       .Replace("[color=paladin]", "")
                       .Replace("[color=warrior]", "")
                       .Replace("[color=hunter]", "")
                       .Replace("[color=shaman]", "")
                       .Replace("[color=rogue]", "")
                       .Replace("[color=druid]", "")
                       .Replace("[color=priest]", "")
                       .Replace("[color=mage]", "")
                       .Replace("[color=warlock]", "")
                       .Replace("[/color]", "")
                       .Replace("[b]", "")
                       .Replace("[/b]", "")
                       .Replace("[i]", "")
                       .Replace("[/i]", "")
                       .Replace("[u]", "")
                       .Replace("[/u]", "")
                       .Replace("[img]", "")
                       .Replace("[/img]", "")
                       .Replace("[quote]", "")
                       .Replace("[/quote]", "");
        }
        #endregion

        #region Замена опасных символов
        public static string SpecSymbolsToView(this string text)
        {
            return text.Replace("&", "&amp;")   // Очень важно, чтобы замена & была в самом начале
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#x27;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\r\n", "<br>")
                       .Replace("\\", "&#x5C");
                       //.Replace("  ", "&nbsp;&nbsp;");
        }

        public static string SpecSymbolsToEdit(this string text)
        {
            return text.Replace("&amp;", "&")
                       .Replace("&quot;", "\"")
                       .Replace("&#x27;", "'")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&#x5C", "\\")
                       .Replace("<br>", "\r\n")
                       .Replace("&nbsp;", " ");
        }
        #endregion

        #region BB-Code
        public static string BbCode(this string text)
        {
            return text
                       // Цвета
                       .Replace("[color=paladin]", "<font color=\"#f58cba\">")
                       .Replace("[color=warrior]", "<font color=\"#c79c6e\">")
                       .Replace("[color=hunter]", "<font color=\"#abd473\">")
                       .Replace("[color=shaman]", "<font color=\"#0070de\">")
                       .Replace("[color=rogue]", "<font color=\"#fff569\">")
                       .Replace("[color=druid]", "<font color=\"#ff7d0a\">")
                       .Replace("[color=priest]", "<font color=\"#ffffff\">")
                       .Replace("[color=mage]", "<font color=\"#69ccf0\">")
                       .Replace("[color=warlock]", "<font color=\"#9482c9\">")
                       .Replace("[/color]", "</font>")
                       // Цитирование
                       .Replace("[quote]", "<div class=\"quote\">")
                       .Replace("[/quote]", "</div>")
                       // Жирный текст
                       .Replace("[b]", "<b>")
                       .Replace("[/b]", "</b>")
                       // Наклонный текст
                       .Replace("[i]", "<i>")
                       .Replace("[/i]", "</i>")
                       // Подчеркнутый текст
                       .Replace("[u]", "<u>")
                       .Replace("[/u]", "</u>")
                       // Вставка изображения ссылкой
                       .Replace("[img]", "<img style=\"max-width:400px; height:auto;\" src=") // max-width надо как-нибудь привязать к ширине таблицы !!!!!!!!!!!!!!!!!!!!!!!
                       .Replace("[/img]", ">");
        }
        #endregion

        #region Хеширование строки
        public static string HashString(this string password)
        {
            MD5 crypto = MD5.Create();
            var result = crypto.ComputeHash(Encoding.UTF8.GetBytes(password));

            return Convert.ToBase64String(result);
        }
        #endregion

    }
}
