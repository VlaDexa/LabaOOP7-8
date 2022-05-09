using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Xml;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
