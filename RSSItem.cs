using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

#nullable enable

namespace LabaOOP7_8
{
    internal class RSSItem
    {
        readonly string? Title;
        readonly Uri? Link;
        readonly string? Description;
        readonly string? Author;
        readonly string? Date;

        public RSSItem(XmlNode node)
        {
            Title = node.SelectSingleNode("title")?.InnerText;
            var linkNode = node.SelectSingleNode("link");
            if (linkNode != null) Link = new Uri(linkNode.InnerText);
            Description = node.SelectSingleNode("description")?.InnerText;
            Author = node.SelectSingleNode("author")?.InnerText;
            Date = node.SelectSingleNode("pubDate")?.InnerText;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(58);
            // Check each property if it is null
            // If not, add it
            if (Title != null) sb.AppendLine($"Заголовок - {Title}");
            if (Link != null) sb.AppendLine($"Ссылка - {Link}");
            if (Description != null) sb.AppendLine($"Описание - {Description}");
            if (Author != null) sb.AppendLine($"Автора - {Author}");
            if (Date != null) sb.AppendLine($"Дата - {Date}");

            return sb.ToString();
        }
    }

    class RSSArticle
    {
        readonly string Title;
        readonly Uri Link;
        readonly string Description;
        readonly List<RSSItem> Items;

        public RSSArticle(XmlNode node)
        {
            Title = node.SelectSingleNode("title").Unwrap().InnerText;
            Link = new Uri(node.SelectSingleNode("link").Unwrap().InnerText);
            Description = node.SelectSingleNode("description").Unwrap().InnerText;
            var items = node.SelectNodes("item");
            Items = new List<RSSItem>(items.Count);
            foreach (XmlNode item in items)
                Items.Add(new RSSItem(item));
        }

        public override string ToString()
        {
            var sb = new StringBuilder(54);
            sb.AppendLine($"Название канала - {Title}");
            sb.AppendLine($"Ссылка на канал - {Link}");
            sb.AppendLine($"Описание канала - {Description}");
            foreach (var item in Items)
            {
                sb.AppendLine(new string('=', 40));
                sb.AppendLine(item.ToString().TrimEnd());
            }

            return sb.ToString();
        }
    }

    static class Extensions
    {
        public static T Unwrap<T>(this T? item)
        where T : class
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            return item;
        }
    }
}
