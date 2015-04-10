using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Windows.UI.Xaml.Media.Imaging;
using ManutdNews.Models;


namespace ManutdNews.Services
{
    public class DataService : IDataService
    {
        private string homePageUrl = "manutd.com.vn";
        private string manutdNewsUrl = "http://manutd.com.vn/category/tin-manchester-united/";
        private string manutdNewsNextPageUrl = "manutd.com.vn/category/tin-manchester-united/page/3/?paged=";
        private HtmlDocument htmlDoc;
        private List<HtmlNode> newsNodes;
        private HtmlWeb htmlWeb;

        public DataService()
        {
            newsNodes = new List<HtmlNode>();
            htmlWeb = new HtmlWeb();
            htmlDoc = new HtmlDocument();
        }

        public void GetNewsDetails(Action<Article, Exception> callback)
        {
            //var url = "http://www.goal.com/vn/news/4867/b%C3%B3ng-%C4%91%C3%A1-anh/2013/09/05/4239342/van-persie-t%C3%A1n-d%C6%B0%C6%A1ng-moyes?ICID=CP_97";
            //webScraper.StartScrapingArticle(url);
            //var item = webScraper.Article;
            //callback(item, null);
        }

        public void GetNewsList(int pageNumber, Action<IEnumerable<Article>, Exception> callback)
        {
            //var url = "http://www.goal.com/vn/teams/england/97/manchester-united/news?ICID=CP_97";
            //webScraper.StartScrapingArticleList(url);
            //var collection = webScraper.ArticleList;
            //callback(collection, null);

            List<Article> newsList;
            if (pageNumber == 1)
                newsList = ScrapeNewsPage(pageNumber, manutdNewsUrl).Result;
            else
                newsList = ScrapeNewsPage(pageNumber, manutdNewsNextPageUrl + pageNumber).Result;
            callback(newsList, null);
        }

        private async Task<List<Article>> ScrapeNewsPage(int pageNumber, string url)
        {
            List<Article> newsList = new List<Article>();
            //IsLoading = true;
            newsNodes.Clear();

            using (var client = new Windows.Web.Http.HttpClient())
            {
                var result = await client.GetStringAsync(new Uri(url, UriKind.Absolute));
                htmlDoc.LoadHtml(result);
            }
            //htmlDoc = await htmlWeb.LoadFromWebAsync(url);

            if (pageNumber == 1)
            {
                // Get big news node
                var bigNewsNode = htmlDoc.DocumentNode.Descendants("div")
                    .Where(o => o.GetAttributeValue("class", "") == "big_news").FirstOrDefault();
                newsNodes.Add(bigNewsNode);
            }
            // Get normal news nodes
            newsNodes.AddRange(htmlDoc.DocumentNode.Descendants("div")
                .Where(o => o.GetAttributeValue("class", "") == "normal_news").ToList());

            //for (int i = 0; i < newsNodes.Count; i++)
            foreach (var node in newsNodes)
            {
                var articleItem = ScrapeNewsNode(node);
                newsList.Add(articleItem);
            }

            //pageNumber++;
            //isLoading = false;
            return newsList;
        }

        private Article ScrapeNewsNode(HtmlNode articleNode)
        {
            Article articleItem;
            if (articleNode.Attributes[0].Value == "big_news")
                articleItem = ParseBigNews(articleNode);
            else
                articleItem = ParseNormalNews(articleNode);

            return articleItem;
        }

        private Article ParseBigNews(HtmlNode articleNode)
        {
            // scraping date
            var timeNode = articleNode.Descendants("p")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "big_news_date");
            var dateString = timeNode.InnerText;
            var extractedDateString = dateString;
            //var extractedDateString = this.DateExtracter(dateString);

            // scraping image url
            var imgNode = articleNode.Descendants("a")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "big_news_img float_left");
            var articleImageUrl = imgNode.FirstChild.Attributes["src"].Value;

            // scraping news link
            var articleUrlString = imgNode.Attributes["href"].Value;

            // scraping article title
            var titleNode = articleNode.Descendants("a")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "big_news_title");
            var titleString = titleNode.InnerText;

            // scraping article summary
            var summaryNode = articleNode.Descendants("div")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "big_news_desc").ChildNodes.ElementAt(3);
            var summaryString = summaryNode.InnerText;

            var articleItem = new Article();
            articleItem.Title = titleString;
            articleItem.Summary = summaryString;
            articleItem.ArticleUrl = this.homePageUrl + articleUrlString;
            var uri = new Uri(articleImageUrl, UriKind.Absolute);
            articleItem.Image = new BitmapImage(uri);
            articleItem.PubDateString = extractedDateString;
            return articleItem;
        }

        private Article ParseNormalNews(HtmlNode articleNode)
        {
            // scraping date
            var timeNode = articleNode.Descendants("p")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "normal_news_date");
            var dateString = timeNode.InnerText;
            var extractedDateString = dateString;
            //var extractedDateString = this.DateExtracter(dateString);

            // scraping news link
            var readMoreNode = articleNode.Descendants("p")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "readmore");
            var articleUrlString = readMoreNode.FirstChild.Attributes["href"].Value;

            // scraping image url
            var imgNode = articleNode.Descendants("a")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "normal_news_img float_left");
            var articleImageUrl = imgNode.FirstChild.Attributes["src"].Value;

            // scraping article title
            var titleNode = articleNode.Descendants("a")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "normal_news_title");
            var titleString = titleNode.InnerText;

            // scraping article summary
            var summaryNode = articleNode.Descendants("div")
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "normal_news_desc").ChildNodes.ElementAt(3);
            var summaryString = summaryNode.InnerText;

            var articleItem = new Article();
            articleItem.Title = titleString;
            articleItem.Summary = summaryString;
            articleItem.ArticleUrl = this.homePageUrl + articleUrlString;
            var uri = new Uri(articleImageUrl, UriKind.Absolute);
            articleItem.Image = new BitmapImage(uri);
            articleItem.PubDateString = extractedDateString;
            return articleItem;
        }


        public IEnumerable<Article> GetSampleNewsList()
        {
            throw new NotImplementedException();
        }
    }
}