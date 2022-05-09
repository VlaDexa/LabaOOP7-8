using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

#nullable enable

namespace LabaOOP7_8
{
    internal class RSSItem
    {
        private readonly string? Title;
        private readonly Uri? Link;
        private readonly string? Description;
        private readonly string? Author;
        private readonly string? Date;

        public RSSItem(XmlNode node)
        {
            Title = node.SelectSingleNode("title")?.InnerText;
            var linkNode = node.SelectSingleNode("link");
            if (linkNode != null) Link = new Uri(linkNode.InnerText);
            Description = node.SelectSingleNode("description")?.InnerText;
            Author = node.SelectSingleNode("author")?.InnerText;
            Date = node.SelectSingleNode("pubDate")?.InnerText;
        }

        public RSSItem(MySqlDataReader connection)
        {
            Title = connection.GetString("title");
            Link = connection.GetString("link") != null ? new Uri(connection.GetString("link")) : null;
            Description = connection.GetString("description");
            Author = connection.GetString("author");
            Date = connection.GetString("date");
        }

        public void WriteToDb(MySqlConnection connection)
        {
            // Create a table if it doesn't exist where every field is a nullable string
            var sql = "CREATE TABLE IF NOT EXISTS news (title MEDIUMTEXT, link MEDIUMTEXT, description MEDIUMTEXT, author MEDIUMTEXT, date MEDIUMTEXT)";
            var command = new MySqlCommand(sql, connection);
            command.ExecuteNonQuery();
            sql = $"INSERT INTO news (title, link, description, author, date) VALUES ('{Title}', '{Link}', '{Description}', '{Author}', '{Date}')";
            command = new MySqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public override string ToString()
        {
            StringBuilder? sb = new StringBuilder(58);
            // Check each property if it is null
            // If not, add it
            if (Title != null)
                sb.AppendLine($"Заголовок - {Title}");

            if (Link != null)
                sb.AppendLine($"Ссылка - {Link}");

            if (Description != null)
                sb.AppendLine($"Описание - {Description}");

            if (Author != null)
                sb.AppendLine($"Автора - {Author}");

            if (Date != null)
                sb.AppendLine($"Дата - {Date}");

            return sb.ToString();
        }
    }

    internal class RSSArticle
    {
        private readonly string Title;
        private readonly Uri Link;
        private readonly string Description;
        private readonly List<RSSItem> Items;

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

        public RSSArticle(MySqlConnection db)
        {
            var sql = "SELECT * FROM article";
            var command = new MySqlCommand(sql, db);
            var reader = command.ExecuteReader();
            reader.Read();
            Title = reader.GetString("title");
            Link = new Uri(reader.GetString("link"));
            Description = reader.GetString("description");
            reader.Close();
            Items = new List<RSSItem>();
            sql = "SELECT * FROM news";
            command = new MySqlCommand(sql, db);
            reader = command.ExecuteReader();
            while (reader.Read())
                Items.Add(new RSSItem(reader));
        }

        public void WriteToDB(MySqlConnection db, string fullText)
        {
            // Create table article if it doesn't exist
            // there can be only one article
            var sql = "CREATE TABLE IF NOT EXISTS article (title MEDIUMTEXT, link MEDIUMTEXT, description MEDIUMTEXT, raw_text LONGTEXT)";
            var command = new MySqlCommand(sql, db);
            command.ExecuteNonQuery();
            // Insert article into table
            sql = $"INSERT INTO article (title, link, description, raw_text) VALUES ('{Title}', '{Link}', '{Description}', '{fullText.Replace("\'", "\\'")}')";
            command = new MySqlCommand(sql, db);
            command.ExecuteNonQuery();
            foreach (var item in Items)
                item.WriteToDb(db);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(54);
            sb.AppendLine($"Название канала - {Title}");
            sb.AppendLine($"Ссылка на канал - {Link}");
            sb.AppendLine($"Описание канала - {Description}");
            foreach (RSSItem? item in Items)
            {
                sb.AppendLine(new string('=', 40));
                sb.AppendLine(item.ToString().TrimEnd());
            }

            return sb.ToString();
        }
    }

    internal static class Extensions
    {
        public static T Unwrap<T>(this T? item)
        where T : class
        {
            return item is null ? throw new ArgumentNullException(nameof(item)) : item;
        }
    }
}
