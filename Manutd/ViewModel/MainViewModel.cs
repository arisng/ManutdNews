using System;
using System.Windows;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

using Manutd.Services;
using Manutd.Models;
using HtmlAgilityPack;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;

namespace Manutd.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private INavigationService navigationService;
        private string baseUrl = "http://www.goal.com";
        private string nextPageUrlString;
        private int selectedPanoramaIndex = 0;
        private int lastScrapedArticleIndex;
        private bool isRefreshing = false;
        private bool isEndOfArticleList = false;
        private HtmlNodeCollection articleNodes;
        private HtmlNode lastArticleNode;
        private HtmlNode nextPageNode;
        private ObservableCollection<Article> articleList;
        private ObservableCollection<Article> collectionOfTen = new ObservableCollection<Article>();
        private ObservableCollection<Article> collection = new ObservableCollection<Article>();

        #endregion

        #region Properties
        
        public ObservableCollection<Article> ArticleList
        {
            get { return this.articleList; }
            set
            {
                if (value == null) return;
                this.articleList = value;
                this.RaisePropertyChanged(() => ArticleList);
            }
        }
        
        public bool IsRefreshing
        {
            get { return this.isRefreshing; }
            set
            {
                this.isRefreshing = value;
                this.RaisePropertyChanged(() => IsRefreshing);
            }
        }

        public int SelectedPanoramaIndex
        {
            get { return this.selectedPanoramaIndex; }
            set
            {
                this.selectedPanoramaIndex = value;
                this.HandleCurrentSectionChanged();
            }
        }

        public RelayCommand LoadMoreCommand { get; set; }
        public RelayCommand<int> NavigateToNewsDetailsPageCommand { get; private set; }

        #endregion

        public MainViewModel(INavigationService navService)
        {
            //this.navigationService = new NavigationService(((App)Application.Current).RootFrame);
            this.navigationService = navService;
            this.ArticleList = new ObservableCollection<Article>();
            if (IsInDesignMode)
                ArticleList.Add(new Article
                {
                    Title = "Hernandez always wanted Madrid",
                    Summary = "The legendary Scottish coach says the Mexican has long had his heart set on playing for Los Blancos and is backing him to succeed at the Santiago Bernabeu",
                    PubDateString = "2014-09-04",
                    Image = new BitmapImage(new Uri("/Resources/Images/415594_heros", UriKind.Relative)),
                });

            var url = "http://www.goal.com/en/teams/england/premier-league/8/manchester-united/662/team-news";
            this.StartScarpingArticleList(url);

            this.LoadMoreCommand = new RelayCommand(this.LoadMoreAction);
            this.NavigateToNewsDetailsPageCommand = new RelayCommand<int>(this.NavigateToNewsDetailsPage);
        }

        #region Private Methods
        
        private void NavigateToNewsDetailsPage(int index)
        {
            var url = this.ArticleList[index].ArticleUrl;
            this.navigationService.NavigateTo("NewsDetails", url);
        }

        private void HandleCurrentSectionChanged()
        {

        }

        private void StartScarpingArticleList(string uri)
        {
            isRefreshing = true;
            HtmlWeb.LoadAsync(uri, OnCallback);
        }

        private void OnCallback(object s, HtmlDocumentLoadCompleted e)
        {
            if (e.Error == null)
            {
                var htmlDoc = e.Document;

                var teamNewsSection = htmlDoc.DocumentNode.SelectSingleNode("//section[@class='team-news']");
                articleNodes = teamNewsSection.SelectNodes("//article[@class='story clearfix']");

                // get next page url
                var paginationDiv = teamNewsSection.SelectSingleNode(".//div[@class='pagination']");
                this.nextPageNode = paginationDiv.SelectSingleNode(".//a[@class='btn next']");
                if (this.nextPageNode != null)
                    this.nextPageUrlString = this.nextPageNode.Attributes["href"].Value;

                // extract article list's content
                this.ScrapeArticleNodes();

                this.ArticleList = collection;
                this.IsRefreshing = false;
            }
        }

        private void LoadMoreAction()
        {
            if (!isRefreshing)
            {
                if (!isEndOfArticleList)
                {
                    this.ScrapeArticleNodes();
                    this.ArticleList = collection;
                    //return;
                }
                else
                    this.StartScarpingArticleList(this.baseUrl + this.nextPageUrlString);
            }
        }

        private void ScrapeArticleNodes()
        {
            for (int i = lastScrapedArticleIndex + 1; i < articleNodes.Count; i++)
            {
                var articleItem = this.ScrapeArticleNode(articleNodes[i]);

                // package article list into pages of 10 articles
                collectionOfTen.Add(articleItem);
                this.lastScrapedArticleIndex = i;

                if (this.lastScrapedArticleIndex == articleNodes.Count - 1)
                {
                    this.isEndOfArticleList = true;
                    this.lastScrapedArticleIndex = -1;
                }
                else
                {
                    this.isEndOfArticleList = false;
                }

                if (collectionOfTen.Count == 10)
                {
                    while (collectionOfTen.Count > 0 || i + 1 == articleNodes.Count)
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
            var result = datestring.Substring(0, 10);

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