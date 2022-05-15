using MySql.Data.MySqlClient;
using System;
using System.Net;
using System.IO;
using System.Windows;
using System.Xml;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LabaOOP7_8.RSS;

namespace LabaOOP7_8
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly XmlDocument document = new XmlDocument();
        static readonly MySqlConnection sqlConnection = new MySqlConnection("server=localhost;user=Laba;port=3306;password=TestetyTest");
        static readonly Regex regexp = new Regex("<[^<]+>", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        
        public MainWindow()
        {
            InitializeComponent();
            sqlConnection.Open();
        }
        
        private string FormatRaw(string input)
        {
            var matches = regexp.Matches(input);
            var sb = new StringBuilder(input, input.Length + matches.Count);
            foreach (var (index, match) in regexp.Matches(input).Cast<Match>().Enumerate())
                sb.Insert(match.Index+match.Length+(int)index, '\n');

            return sb.ToString();
        }

        private void DownloadRss(object sender, RoutedEventArgs e)
        {
            var rssLinkText = RSSLink.Text;
            try
            {
                document.Load(rssLinkText);
                var formated = FormatRaw(document.InnerXml);
                Raw.Document.Blocks.Clear();
                Raw.AppendText(formated);
                var channelNode = document.SelectSingleNode("rss/channel").Unwrap();
                var article = new Article(channelNode);
                Parsed.Document.Blocks.Clear();
                Parsed.AppendText(article.ToString());
                const string sql = "DROP DATABASE IF EXISTS news; CREATE DATABASE news; USE news; CREATE TABLE IF NOT EXISTS article (title MEDIUMTEXT, link MEDIUMTEXT, description MEDIUMTEXT, raw_text LONGTEXT); CREATE TABLE IF NOT EXISTS news (title MEDIUMTEXT, link MEDIUMTEXT, description MEDIUMTEXT, author MEDIUMTEXT, date MEDIUMTEXT);";
                var command = new MySqlCommand(sql, sqlConnection);
                command.ExecuteNonQuery();
                article.WriteToDb(sqlConnection, document.InnerXml);
            }
            catch (WebException)
            {
                MessageBox.Show("Невозможно подключиться к сайту");
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                MessageBox.Show("Данный файл не найден");
            }
            catch (XmlException)
            {
                MessageBox.Show("Данные на сайте не являются валидным XML");
            }
            catch (MySqlException)
            {
                MessageBox.Show("Ошибка при записи в БД");
            }
            catch
            {
                MessageBox.Show("Неизвестная ошибка");
            }
        }

        private void LoadFromDb(object sender, RoutedEventArgs e)
        {
            try
            {
                const string sql = "USE news; SELECT * FROM article";
                var command = new MySqlCommand(sql, sqlConnection);
                var reader = command.ExecuteReader();
                reader.Read();
                Raw.Document.Blocks.Clear();
                Raw.AppendText(reader.GetString("raw_text"));
                reader.Close();
                var article = new Article(sqlConnection);
                Parsed.Document.Blocks.Clear();
                Parsed.AppendText(article.ToString());
            }
            catch (MySqlException)
            {
                MessageBox.Show("Невозможно подключиться к базе данных");
            }
        }
    }

    internal static partial class Extensions
    {
        public static IEnumerable<(ulong, T)> Enumerate<T>(this IEnumerable<T> ts)
        {
            ulong index = 0;
            foreach (var val in ts)
                yield return (index++, val);
            yield break;
        }
    }
}
