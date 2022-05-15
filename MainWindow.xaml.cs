using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Xml;
using LabaOOP7_8.RSS;

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

        private static string DownloadStringFromLink(string link)
        {
            var request = WebRequest.Create(link);
            using var response = request.GetResponse();
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private void DownloadRss(object sender, RoutedEventArgs e)
        {
            var rssLinkText = RSSLink.Text;
            try
            {
                var rssXmlString = DownloadStringFromLink(rssLinkText);
                Raw.Document.Blocks.Clear();
                Raw.AppendText(rssXmlString);
                var document = new XmlDocument();
                document.LoadXml(rssXmlString);
                var channelNode = document.SelectSingleNode("rss/channel").Unwrap();
                var article = new Article(channelNode);
                Parsed.Document.Blocks.Clear();
                Parsed.AppendText(article.ToString());
                using var sqlConnection = new MySqlConnection("server=localhost;user=Laba;port=3306;password=TestetyTest");
                sqlConnection.Open();
                const string sql = "DROP DATABASE IF EXISTS news; CREATE DATABASE news; USE news;";
                var command = new MySqlCommand(sql, sqlConnection);
                command.ExecuteNonQuery();
                article.WriteToDb(sqlConnection, rssXmlString);
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

        private void LoadFromDb(object sender, RoutedEventArgs e)
        {
            try
            {
                using var sqlConnection = new MySqlConnection("server=localhost;user=Laba;port=3306;password=TestetyTest;database=news");
                sqlConnection.Open();
                const string sql = "SELECT * FROM article";
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
}
