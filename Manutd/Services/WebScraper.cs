using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Manutd.Models;
using Manutd.ViewModel;
using Manutd;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Manutd.Services
{
    public class WebScraper
    {
        #region Fields
        
        private static WebScraper webScraper;
        private string baseUrl = "http://www.goal.com";
        private string nextPageUrlString;
        private int lastScrapedArticleIndex;
        private bool isRefreshing = false;
        private bool isLoading = false;
        private bool isEndOfArticleList = false;
        private ObservableCollection<Article> collectionOfTen = new ObservableCollection<Article>();
        private ObservableCollection<Article> collection = new ObservableCollection<Article>();
        private HtmlNodeCollection articleNodes;
        private HtmlNode lastArticleNode;
        private HtmlNode nextPageNode;

        #endregion

        private WebScraper()
        {
            this.Article = new Article();
            this.ArticleList = new ObservableCollection<Article>();
        }

        public static WebScraper GetInstance()
        {
            if (webScraper == null)
                webScraper = new WebScraper();
            return webScraper;
        }

        #region Properties

        public enum ContentType
        {
            Text,
            Image
        }
        public Article Article { get; set; }
        public ObservableCollection<Article> ArticleList { get; set; }
        public bool IsRefreshing { get; set; }

        #endregion


        #region Public Methods

        public void StartScrapingArticleList(string url)
        {
            isRefreshing = true;
            HtmlWeb.LoadAsync(url, OnCallbackArticleList);
        }

        public void LoadMoreAction()
        {
            if (!isRefreshing)
            {
                if (!isEndOfArticleList)
                {
                    this.ScrapeArticleNodesList(this.lastScrapedArticleIndex);

                    this.ArticleList = collection;
                    //return;
                }
                else
                    this.StartScrapingArticleList(this.baseUrl + this.nextPageUrlString);
            }
        }

        public void StartScrapingArticle(string url)
        {
            isLoading = true;
            HtmlWeb.LoadAsync(url, OnCallback);
        }

        #endregion


        #region Private Methods

        #region Article List

        private void OnCallbackArticleList(object s, HtmlDocumentLoadCompleted e)
        {
            if (e.Error == null)
            {
                var htmlDoc = e.Document;

                var teamNewsSection = htmlDoc.DocumentNode.SelectSingleNode("//section[@class='team-news']");
                articleNodes = teamNewsSection.SelectNodes("//article[@class='story clearfix']");

                // get next page url
                var paginationDiv = teamNewsSection.SelectSingleNode(".//div[@class='pagination']");
                nextPageNode = paginationDiv.SelectSingleNode(".//a[@class='btn next']");
                if (nextPageNode != null)
                    nextPageUrlString = this.nextPageNode.Attributes["href"].Value;

                // extract article list's content
                this.ScrapeArticleNodesList(0);

                ArticleList = collection;
                IsRefreshing = false;
            }
        }

        private void ScrapeArticleNodesList(int lastIndex)
        {
            for (int i = lastScrapedArticleIndex + 1; i < articleNodes.Count; i++)
            {
                var articleItem = this.ScrapeArticleNode(articleNodes[i]);

                // package article list into pages of 10 articles
                collectionOfTen.Add(articleItem);
                this.lastScrapedArticleIndex = i;

                if (this.lastScrapedArticleIndex == articleNodes.Count - 1)
                {
                    //this.isStartFromBeginingOfArticleList = true;
                    this.isEndOfArticleList = true;
                    this.lastScrapedArticleIndex = -1;
                    break;
                }
                else
                {
                    //this.isStartFromBeginingOfArticleList = false;
                    this.isEndOfArticleList = false;
                }

                if (collectionOfTen.Count == 10)
                {
                    while (collectionOfTen.Count > 0)
                    {
                        collection.Add(collectionOfTen[0]);
                        collectionOfTen.RemoveAt(0);
                    }
                }

                if (this.collectionOfTen.Count == 0)
                    return;
            }

            //if (!isEndOfArticleList)
            //    LoadMoreAction();
        }

        private Article ScrapeArticleNode(HtmlNode articleNode)
        {
            this.lastArticleNode = articleNode;

            // scraping date
            var timeNode = articleNode.SelectSingleNode(".//time[@class='time']");
            var dateString = timeNode.Attributes["datetime"].Value;
            var extractedDateString = this.DateExtracter(dateString);

            // scraping article url
            var linkNode = articleNode.SelectSingleNode(".//a");
            var articleUrlString = linkNode.Attributes["href"].Value;

            // scraping image url
            var imgNode = articleNode.SelectSingleNode(".//img");
            var articleImageUrlString = imgNode.Attributes["src"].Value;

            // scraping article title
            var titleNode = articleNode.SelectSingleNode(".//a[@class='font-reset']");
            var titleString = titleNode.InnerText;

            // scraping article summary
            var summaryNode = articleNode.SelectSingleNode(".//div[@class='summary']");
            var summaryString = summaryNode.InnerText;

            var articleItem = new Article();
            articleItem.Title = titleString;
            articleItem.Summary = summaryString;
            articleItem.ArticleUrl = this.baseUrl + articleUrlString;
            var uri = new Uri(articleImageUrlString, UriKind.Absolute);
            articleItem.Image = new BitmapImage(uri);
            articleItem.PubDateString = extractedDateString;

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

        #endregion

        #region Article Details

        private void OnCallback(object s, HtmlDocumentLoadCompleted e)
        {
            var loadedArticle = new Article();
            if (e.Error == null)
            {
                var htmlDoc = e.Document;

                // get article head lines
                loadedArticle.Title = htmlDoc.DocumentNode.SelectSingleNode("//h1[@itemprop='headline']").InnerText;

                // get article content
                var articleTextDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='article-text']");
                var innerText = articleTextDiv.InnerText;
                var innerHtml = articleTextDiv.InnerHtml;
                var startArticleNode = articleTextDiv.SelectSingleNode(".//comment()");
                var currentNode = startArticleNode.NextSibling;
                var count = 0;
                while (!currentNode.InnerText.Contains("Article End"))
                {
                    var nodeName = currentNode.Name;
                    switch (nodeName)
                    {
                        case "p":
                            if (currentNode.NextSibling.Name.Equals("img"))
                            {
                                var uri = new Uri(currentNode.Attributes["src"].Value);
                                loadedArticle.Contents.Add(new Content(new BitmapImage(uri)));
                            }
                            else
                                loadedArticle.Contents.Add(new Content(currentNode.InnerText + "\n\n"));
                            break;
                        case "#text":
                            loadedArticle.Contents.Add(new Content(currentNode.InnerText));
                            break;
                        case "a":
                            loadedArticle.Contents.Add(new Content(currentNode.InnerText));
                            break;
                        case "br":
                            loadedArticle.Contents.Add(new Content("\n"));
                            break;
                    }
                    currentNode = currentNode.NextSibling;
                    count++;
                }

                // get article author
                //loadedArticle.Author = htmlDoc.DocumentNode.SelectSingleNode("//h2[@class='author']").InnerText;

                // get article pub Date
                loadedArticle.PubDateString = htmlDoc.DocumentNode.SelectSingleNode("//time[@itemprop='datePublished']").InnerText;

                // get article images
                var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[@class=' article-image']");
                var imageUrlString = imageNode.Attributes["src"].Value;
                var url = new Uri(imageUrlString);
                loadedArticle.Image = new BitmapImage(url);

                // get article source

                this.Article = loadedArticle;
                isLoading = false;
            }
        }

        #endregion

        private string DateExtracter(string datestring)
        {
            var result = datestring.Substring(0, 10);
            return result;
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
        }

        #endregion
    }
}
