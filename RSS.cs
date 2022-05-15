#nullable enable

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LabaOOP7_8
{
    namespace RSS
    {
        internal class Item
        {
            private readonly string? Author;
            private readonly string? Date;
            private readonly string? Description;
            private readonly string? Link;
            private readonly string? Title;

            public Item(XmlNode node)
            {
                Title = node.SelectSingleNode("title")?.InnerText;
                Link = node.SelectSingleNode("link")?.InnerText;
                Description = node.SelectSingleNode("description")?.InnerText;
                Author = node.SelectSingleNode("author")?.InnerText;
                Date = node.SelectSingleNode("pubDate")?.InnerText;
            }

            private static string? CheckExistenceAndReturn(MySqlDataReader db, string column)
            {
                return db.IsDBNull(db.GetOrdinal(column)) ? null : db.GetString(column);
            }

            public Item(MySqlDataReader connection)
            {
                Title = CheckExistenceAndReturn(connection, "title");
                Link = CheckExistenceAndReturn(connection, "link");
                Description = CheckExistenceAndReturn(connection, "description");
                Author = CheckExistenceAndReturn(connection, "author");
                Date = CheckExistenceAndReturn(connection, "date");
            }

            public void WriteToDb(MySqlConnection connection)
            {
                const string sql =
                    "INSERT INTO news (title, link, description, author, date) VALUES (@title, @link, @description, @author, @date)";
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@title", Title);
                command.Parameters.AddWithValue("@link", Link);
                command.Parameters.AddWithValue("@description", Description);
                command.Parameters.AddWithValue("@author", Author);
                command.Parameters.AddWithValue("@date", Date);
                command.ExecuteNonQuery();
            }

            public override string ToString()
            {
                var sb = new StringBuilder(58);
                // Check each property if it is null
                // If not, add it
                if (Title != null)
                    sb.AppendFormat("Заголовок - {0}\n", Title);

                if (Link != null)
                    sb.AppendFormat("Ссылка - {0}\n", Link);

                if (Description != null)
                    sb.AppendFormat("Описание - {0}\n", Description);

                if (Author != null)
                    sb.AppendFormat("Автор - {0}\n", Author);

                if (Date != null)
                    sb.AppendFormat("Дата - {0}\n", Date);

                return sb.ToString();
            }
        }

        internal class Article
        {
            private readonly string Description;
            private readonly List<Item> Items;
            private readonly string Link;
            private readonly string Title;

            public Article(XmlNode node)
            {
                Title = node.SelectSingleNode("title").Unwrap().InnerText;
                Link = node.SelectSingleNode("link").Unwrap().InnerText;
                Description = node.SelectSingleNode("description").Unwrap().InnerText;
                Items = node.SelectNodes("item").Unwrap().Cast<XmlNode>().Select(item => new Item(item)).ToList();
            }

            public Article(MySqlConnection db)
            {
                var sql = "SELECT * FROM article";
                var command = new MySqlCommand(sql, db);
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    Title = reader.GetString("title");
                    Link = reader.GetString("link");
                    Description = reader.GetString("description");
                }
                sql = "SELECT * FROM news";
                command = new MySqlCommand(sql, db);
                Items = new List<Item>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        Items.Add(new Item(reader));
                }
            }

            public void WriteToDb(MySqlConnection db, string fullText)
            {
                const string sql =
                    "INSERT INTO article (title, link, description, raw_text) VALUES (@title, @link, @description, @raw_text)";
                var command = new MySqlCommand(sql, db);
                command.Parameters.AddWithValue("@title", Title);
                command.Parameters.AddWithValue("@link", Link);
                command.Parameters.AddWithValue("@description", Description);
                command.Parameters.AddWithValue("@raw_text", fullText);
                command.ExecuteNonQuery();
                foreach (var item in Items)
                    item.WriteToDb(db);
            }

            public override string ToString()
            {
                var sb = new StringBuilder(54);
                sb.AppendFormat("Название канала - {0}\n", Title);
                sb.AppendFormat("Ссылка на канал - {0}\n", Link);
                sb.AppendFormat("Описание канала - {0}\n", Description);
                foreach (var item in Items)
                {
                    sb.AppendLine(new string('=', 40));
                    sb.AppendLine(item.ToString().TrimEnd());
                }

                return sb.ToString();
            }
        }
    }

    internal static partial class Extensions
    {
        public static T Unwrap<T>(this T item)
            where T : class
        {
            return item is null ? throw new ArgumentNullException(nameof(item)) : item;
        }
    }
}