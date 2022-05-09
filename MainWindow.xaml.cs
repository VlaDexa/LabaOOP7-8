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
            var RSSString = RSSLink.Text;
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
            }
            catch (WebException)
            {
                MessageBox.Show("Невозможно подключиться к сайту");
            }
            catch (XmlException)
            {
                MessageBox.Show("Данные на сайте не являются валидным XML");
            }
            catch
            {
                MessageBox.Show("Неизвестная ошибка");
            }
        }
    }
}
