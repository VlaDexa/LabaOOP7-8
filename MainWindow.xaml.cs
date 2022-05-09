using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Xml;

namespace LabaOOP7_8
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DownloadRss(object sender, RoutedEventArgs e)
        {
            string RSSString = RSSLink.Text;
            try
            {
                var RSSXML = WebRequest.Create(RSSString);
                using var response = RSSXML.GetResponse();
                using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream);
                var RSSXMLString = reader.ReadToEnd();
                reader.Close();
                stream.Close();
                response.Close();
                Raw.Document.Blocks.Clear();
                Raw.AppendText(RSSXMLString);
                var RSSXMLDocument = new XmlDocument();
                RSSXMLDocument.LoadXml(RSSXMLString);
                var channelNode = RSSXMLDocument.SelectSingleNode("rss/channel").Unwrap();
                var RSSNews = new RSSArticle(channelNode);
                Parsed.Document.Blocks.Clear();
                Parsed.AppendText(RSSNews.ToString());
                using var sqlConnection = new MySqlConnection("server=localhost;user=Laba;port=3306;password=TestetyTest");
                sqlConnection.Open();
                // Drop database news if exists
                var sql = "DROP DATABASE IF EXISTS news";
                var command = new MySqlCommand(sql, sqlConnection);
                command.ExecuteNonQuery();
                // Create database news
                sql = "CREATE DATABASE news";
                command = new MySqlCommand(sql, sqlConnection);
                command.ExecuteNonQuery();
                // Use database news
                sql = "USE news";
                command = new MySqlCommand(sql, sqlConnection);
                command.ExecuteNonQuery();
                RSSNews.WriteToDB(sqlConnection, RSSXMLString);
            }
            catch (WebException)
            {
                MessageBox.Show("Невозможно подключиться к сайту");
            }
            catch (XmlException)
            {
                MessageBox.Show("Данные на сайте не являются валидным XML");
            }
            catch (MySqlException)
            {
                MessageBox.Show("Ошибка при записи в БД");
            }
            catch (Exception)
            {
                MessageBox.Show("Неизвестная ошибка");
            }
        }

        private void LoadFromDB(object sender, RoutedEventArgs e)
        {
            try
            {
                using var sqlConnection = new MySqlConnection("server=localhost;user=Laba;port=3306;password=TestetyTest;database=news");
                sqlConnection.Open();
                var sql = "SELECT * FROM article";
                var command = new MySqlCommand(sql, sqlConnection);
                var reader = command.ExecuteReader();
                reader.Read();
                Raw.Document.Blocks.Clear();
                Raw.AppendText(reader.GetString("raw_text"));
                reader.Close();
                var article = new RSSArticle(sqlConnection);
                Parsed.Document.Blocks.Clear();
                Parsed.AppendText(article.ToString());
            }
            catch (MySqlException)
            {
                MessageBox.Show("Невозможно подключиться к базе данных");
            }
        }
    }
}
