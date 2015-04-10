using System;
using Manutd.Models;
using System.Collections.ObjectModel;


namespace Manutd.Services
{
    public class DataService : IDataService
    {
        private WebScraper webScraper;

        public DataService()
        {
            this.webScraper = WebScraper.GetInstance();
        }

        public void GetData(Action<DataItem, Exception> callback)
        {
        //    // Use this to connect to the actual data service

        //    //var item = new DataItem("Welcome to MVVM Light");

        //    var rt = new WebScrapingService("http://www.manutd.com/en/News-And-Features/Football-News/2013/Sep/Nemanja-Vidic-reacts-to-Liverpool-1-United-0.aspx");
        //    //while (rt.TaskCompleted == false)
        //    //    System.Console.Write("task is not completed");
        //    var item = rt.DataItem;
        //    callback(item, null);
        }

        public void GetArticleItem(Action<Article, Exception> callback)
        {
            var url = "http://www.goal.com/vn/news/4867/b%C3%B3ng-%C4%91%C3%A1-anh/2013/09/05/4239342/van-persie-t%C3%A1n-d%C6%B0%C6%A1ng-moyes?ICID=CP_97";
            webScraper.StartScrapingArticle(url);
            var item = webScraper.Article;
            callback(item, null);
        }

        public void GetArticleCollection(Action<ObservableCollection<Article>, Exception> callback)
        {
            var url = "http://www.goal.com/vn/teams/england/97/manchester-united/news?ICID=CP_97";
            webScraper.StartScrapingArticleList(url);
            var collection = webScraper.ArticleList;
            callback(collection, null);
        }
    }
}