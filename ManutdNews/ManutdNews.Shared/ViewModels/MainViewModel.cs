using System;
using System.Windows;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using HtmlAgilityPack;
using ManutdNews.Models;
using System.Collections.Generic;
using System.Collections.Specialized;
using ManutdNews.Services;

namespace ManutdNews.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private INavigationService navigationService;
        private IDataService dataService;
        //private string baseUrl = "http://www.goal.com";
        private string homePageUrl = "manutd.com.vn";
        private string manutdNewsUrl = "http://manutd.com.vn/category/tin-manchester-united/";
        private string manutdNewsNextPageUrl = "manutd.com.vn/category/tin-manchester-united/page/3/?paged=";
        private int pageNumber = 1;
        private int selectedPanoramaIndex = 0;
        //private int lastScrapedArticleIndex;
        private bool isLoading = false;
        //private bool isEndOfArticleList = false;
        //private HtmlNodeCollection articleNodes;
        private List<HtmlNode> newsNodes;
        private HtmlNode lastArticleNode;
        //private HtmlNode nextPageNode;
        private ObservableCollection<Article> articleList;
        //private List<Article> collectionOfTen;
        private ObservableCollection<Article> collection;
        //private HtmlWeb htmlWeb;
        private HtmlDocument htmlDoc;

        #endregion

        #region Properties

        public ObservableCollection<Article> NewsList
        {
            get { return this.articleList; }
            set
            {
                if (value == null) return;
                this.articleList = value;
                this.RaisePropertyChanged(() => NewsList);
            }
        }
        public bool IsLoading
        {
            get { return this.isLoading; }
            set
            {
                this.isLoading = value;
                this.RaisePropertyChanged(() => IsLoading);
            }
        }
        public RelayCommand LoadMoreCommand { get; set; }
        public RelayCommand<int> NewsItemClickCommand { get; private set; }

        #endregion

        public MainViewModel(INavigationService navigationService, IDataService dataService)
        {
            this.navigationService = navigationService;
            this.dataService = dataService;

            NewsList = new ObservableCollection<Article>();
            NewsList.CollectionChanged += NewsList_CollectionChanged;

            if (IsInDesignMode)
            {
                var newsItems = dataService.GetSampleNewsList();
                NewsList = new ObservableCollection<Article>(newsItems);
            }
            else
            {
                IsLoading = true;
                //dataService.GetNewsList(1, OnCallBack);
            }

            htmlDoc = new HtmlDocument();
            newsNodes = new List<HtmlNode>();
            collection = new ObservableCollection<Article>();
            this.StartScarpingNews(manutdNewsUrl);

            LoadMoreCommand = new RelayCommand(LoadMoreAction);
            NewsItemClickCommand = new RelayCommand<int>(NavigateToNewsDetailsPage);
        }

        #region Private Methods

        private void OnCallBack(IEnumerable<Article> newsItems, Exception e)
        {
            if (e == null)
            {
                IsLoading = false;
                NewsList.ToList().AddRange(newsItems);
            }
        }

        private void NewsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                RaisePropertyChanged(() => NewsList);
            }
        }

        private void NavigateToNewsDetailsPage(int index)
        {
            var url = NewsList[index].ArticleUrl;
            navigationService.NavigateTo("NewsDetails", url);
        }

        private void StartScarpingNews(string url)
        {
            //var htmlDoc = await htmlWeb.LoadFromWebAsync(uri);

            ScrapeNewsPage(url);

            //this.ArticleList = collection;
        }

        //private void OnCallback(object s, HtmlDocumentLoadCompleted e)
        //{
        //    if (e.Error == null)
        //    {
        //        var htmlDoc = e.Document;

        //        var teamNewsSection = htmlDoc.DocumentNode.SelectSingleNode("//section[@class='team-news']");
        //        articleNodes = teamNewsSection.SelectNodes("//article[@class='story clearfix']");

        //        // get next page url
        //        var paginationDiv = teamNewsSection.SelectSingleNode(".//div[@class='pagination']");
        //        this.nextPageNode = paginationDiv.SelectSingleNode(".//a[@class='btn next']");
        //        if (this.nextPageNode != null)
        //            this.nextPageUrlString = this.nextPageNode.Attributes["href"].Value;

        //        // extract article list's content
        //        this.ScrapeArticleNodes();

        //        this.ArticleList = collection;
        //        this.IsRefreshing = false;
        //    }
        //}

        private void LoadMoreAction()
        {
            if (!IsLoading)
                //dataService.GetNewsList(++pageNumber, OnCallBack);
                ScrapeNewsPage(manutdNewsNextPageUrl + pageNumber);
        }

        private async void ScrapeNewsPage(string url)
        {
            IsLoading = true;
            newsNodes.Clear();

            using (var client = new Windows.Web.Http.HttpClient())
            {
                var result = await client.GetStringAsync(new Uri(url, UriKind.Absolute));
                htmlDoc.LoadHtml(result);
            }

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

            for (int i = 0; i < newsNodes.Count; i++)
            {
                var articleItem = ScrapeNewsNode(newsNodes[i]);
                NewsList.Add(articleItem);

                // package article list into pages of 10 articles
                //collectionOfTen.Add(articleItem);
                //this.lastScrapedArticleIndex = i;

                //if (this.lastScrapedArticleIndex == articleNodes.Count - 1)
                //{
                //    this.isEndOfArticleList = true;
                //    this.lastScrapedArticleIndex = -1;
                //}
                //else
                //{
                //    this.isEndOfArticleList = false;
                //}

                //if (collectionOfTen.Count == 10)
                //{
                //    while (collectionOfTen.Count > 0 || i + 1 == articleNodes.Count)
                //    {
                //        collection.Add(collectionOfTen[0]);
                //        collectionOfTen.RemoveAt(0);
                //    }
                //}

                //if (this.collectionOfTen.Count == 0)
                //    return;
            }

            pageNumber++;
            isLoading = false;
        }

        private Article ScrapeNewsNode(HtmlNode articleNode)
        {
            this.lastArticleNode = articleNode;
            Article articleItem;
            if (articleNode.Attributes[0].Value == "big_news")
                articleItem = ParseBigNews(articleNode);
            else
                articleItem = ParseNormalNews(articleNode);

            //var liNodes = ulDiv.SelectNodes(".//li");
            //if (this.isFromBeginingOfLiNodeCollection)
            //    this.ExtractArticleContent(0, liNodes, extractedDateString);
            //else
            //{
            //    if (this.lastLiNodeIndex == liNodes.Count - 1)
            //        return;
            //    this.lastLiNodeIndex++;
            //    this.ExtractArticleContent(this.lastLiNodeIndex, liNodes, extractedDateString);
            //}
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
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "big_news_desc").ChildNodes.ElementAt(5);
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
                .FirstOrDefault(o => o.GetAttributeValue("class", "") == "normal_news_desc").ChildNodes.ElementAt(5);
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

        private string DateExtracter(string datestring)
        {
            // extract date
            //int start = datestring.IndexOf(',') + 7;
            //int end = start + 1;
            //var result = datestring.Substring(start, end);

            //datestring = datestring.Replace(",", String.Empty);
            //var tokens = datestring.Split(' ');
            //var date = tokens[2];
            //var month = tokens[1];
            //var year = tokens[3];

            //var year = datestring.Substring(0, 4);
            //var month = datestring.Substring(5, 2);
            //var date = datestring.Substring(8, 2);
            var result = datestring.Substring(15, datestring.Length - 1);

            //switch (month)
            //{
            //    case "January":
            //        month = "01";
            //        break;
            //    case "February":
            //        month = "02";
            //        break;
            //    case "March":
            //        month = "03";
            //        break;
            //    case "April":
            //        month = "04";
            //        break;
            //    case "May":
            //        month = "05";
            //        break;
            //    case "June":
            //        month = "06";
            //        break;
            //    case "July":
            //        month = "07";
            //        break;
            //    case "August":
            //        month = "08";
            //        break;
            //    case "September":
            //        month = "09";
            //        break;
            //    case "October":
            //        month = "10";
            //        break;
            //    case "November":
            //        month = "11";
            //        break;
            //    case "December":
            //        month = "12";
            //        break;
            //    default:
            //        break;
            //}

            //switch (month)
            //{
            //    case "một":
            //        month = "01";
            //        break;
            //    case "hai":
            //        month = "02";
            //        break;
            //    case "ba":
            //        month = "03";
            //        break;
            //    case "bốn":
            //        month = "04";
            //        break;
            //    case "năm":
            //        month = "05";
            //        break;
            //    case "sáu":
            //        month = "06";
            //        break;
            //    case "bảy":
            //        month = "07";
            //        break;
            //    case "tám":
            //        month = "08";
            //        break;
            //    case "chín":
            //        month = "09";
            //        break;
            //    case "mười":
            //        month = "10";
            //        break;
            //    case "mười một":
            //        month = "11";
            //        break;
            //    case "mười hai":
            //        month = "12";
            //        break;
            //    default:
            //        break;
            //}

            //var result = date + "/" + month + "/" + year;

            return result;
        }

        #endregion
    }
}
