using System;
using System.Windows;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Media.Imaging;

using ManutdNews.Services;
using ManutdNews.Models;
using HtmlAgilityPack;
using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Windows.UI.Xaml;

namespace ManutdNews.ViewModels
{
    public class NewsDetailsViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private Article article;
        private bool isLoading;
        //private string articleUrl;
        private ObservableCollection<Content> contents;
        private HtmlWeb htmlWeb = new HtmlWeb();

        #region Properties

        public enum ContentType
        {
            Text,
            Image
        }

        public Article Article
        {
            get { return this.article; }
            set
            {
                if (value == null) return;
                this.article = value;
                this.RaisePropertyChanged(() => Article);
            }
        }

        public ObservableCollection<Content> Contents
        {
            get { return this.contents; }
            set
            {
                if (value == null) return;
                this.contents = value;
                this.RaisePropertyChanged(() => Contents);
            }
        }

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                this.RaisePropertyChanged(() => IsLoading);
            }
        }

        #endregion

        public NewsDetailsViewModel(INavigationService navService)
        {
            this.navigationService = navService;
            this.Article = new Article();
        }

        public async void StartScrapingArticle(string url)
        {
            IsLoading = true;
            HtmlDocument htmlDoc = new HtmlDocument();
            using (var client = new Windows.Web.Http.HttpClient())
            {
                var result = await client.GetStringAsync(new Uri(url, UriKind.Absolute));
                htmlDoc.LoadHtml(result);
            }
            var loadedArticle = new Article();
            var contents = new ObservableCollection<Content>();

            // get article head lines
            loadedArticle.Title = htmlDoc.DocumentNode.Element("//h1[@itemprop='headline']").InnerText;
            //contents.Add(new Content(htmlDoc.DocumentNode.SelectSingleNode("//h1[@itemprop='headline']").InnerText));

            // get article pub Date
            //loadedArticle.PubDateString = htmlDoc.DocumentNode.SelectSingleNode("//time[@itemprop='datePublished']").InnerText;
            contents.Add(new Content(htmlDoc.DocumentNode.Element("//time[@itemprop='datePublished']").InnerText));

            // get article images
            var imageNode = htmlDoc.DocumentNode.Element("//img[@class=' article-image']");
            var imageUrlString = imageNode.Attributes["src"].Value;
            var uri = new Uri(imageUrlString);
            //loadedArticle.Image = new BitmapImage(url);
            contents.Add(new Content(new BitmapImage(uri)));

            // get article source

            // get article content
            var articleTextDiv = htmlDoc.DocumentNode.Element("//div[@class='article-text']");
            var innerText = articleTextDiv.InnerText;
            var innerHtml = articleTextDiv.InnerHtml;
            var startArticleNode = articleTextDiv.Element(".//comment()");
            var currentNode = startArticleNode.NextSibling;
            var count = 0;
            while (!currentNode.InnerText.Contains("Article End"))
            {
                var nodeName = currentNode.Name;
                switch (nodeName)
                {
                    case "p":
                        if (currentNode.FirstChild.Name.Equals("img"))
                        {
                            uri = new Uri(currentNode.FirstChild.Attributes["src"].Value);
                            contents.Add(new Content(new BitmapImage(uri)));
                        }
                        else
                            contents.Add(new Content(currentNode.InnerText + "\n"));
                        break;
                    case "#text":
                        var text = "";
                        while (!currentNode.Name.Equals("br")
                            && !currentNode.Name.Equals("table")
                            && !currentNode.Name.Equals("span")
                            && !currentNode.InnerText.Equals("/r/n"))
                        {
                            text += currentNode.InnerText;
                            currentNode = currentNode.NextSibling;
                            if (currentNode.InnerText.Contains("Article End"))
                                break;
                        }
                        if (!string.IsNullOrEmpty(text) && !text.Equals("/r/n"))
                            contents.Add(new Content(text));
                        break;
                    case "a":
                        text = "";
                        while (!currentNode.Name.Equals("br"))
                        {
                            text += currentNode.InnerText;
                            currentNode = currentNode.NextSibling;
                        }
                        contents.Add(new Content(text));
                        break;
                    case "em":
                        if (currentNode.FirstChild.Name.Equals("img"))
                        {
                            uri = new Uri(currentNode.FirstChild.Attributes["src"].Value);
                            contents.Add(new Content(new BitmapImage(uri)));
                        }
                        else
                        {
                            text = "";
                            while (!currentNode.Name.Equals("br"))
                            {
                                text += currentNode.InnerText;
                                currentNode = currentNode.NextSibling;
                                if (currentNode.InnerText.Contains("Article End"))
                                    break;
                            }
                            contents.Add(new Content(text));
                        }
                        break;
                    //case "br":
                    //    contents.Add(new Content("\n"));
                    //break;
                }
                if (currentNode.InnerText.Contains("Article End"))
                    break;
                currentNode = currentNode.NextSibling;
                count++;
            }

            // get article author
            //loadedArticle.Author = htmlDoc.DocumentNode.SelectSingleNode("//h2[@class='author']").InnerText;

            this.Article = loadedArticle;
            this.Contents = contents;
            IsLoading = false;
        }

        //private void OnCallback(object s, HtmlDocumentLoadCompleted e)
        //{
        //    var loadedArticle = new Article();
        //    var contents = new ObservableCollection<Content>();
        //    if (e.Error == null)
        //    {
        //        var htmlDoc = e.Document;

        //        // get article head lines
        //        loadedArticle.Title = htmlDoc.DocumentNode.SelectSingleNode("//h1[@itemprop='headline']").InnerText;
        //        //contents.Add(new Content(htmlDoc.DocumentNode.SelectSingleNode("//h1[@itemprop='headline']").InnerText));

        //        // get article pub Date
        //        //loadedArticle.PubDateString = htmlDoc.DocumentNode.SelectSingleNode("//time[@itemprop='datePublished']").InnerText;
        //        contents.Add(new Content(htmlDoc.DocumentNode.SelectSingleNode("//time[@itemprop='datePublished']").InnerText));

        //        // get article images
        //        var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[@class=' article-image']");
        //        var imageUrlString = imageNode.Attributes["src"].Value;
        //        var url = new Uri(imageUrlString);
        //        //loadedArticle.Image = new BitmapImage(url);
        //        contents.Add(new Content(new BitmapImage(url)));

        //        // get article source

        //        // get article content
        //        var articleTextDiv = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='article-text']");
        //        var innerText = articleTextDiv.InnerText;
        //        var innerHtml = articleTextDiv.InnerHtml;
        //        var startArticleNode = articleTextDiv.SelectSingleNode(".//comment()");
        //        var currentNode = startArticleNode.NextSibling;
        //        var count = 0;
        //        while (!currentNode.InnerText.Contains("Article End"))
        //        {
        //            var nodeName = currentNode.Name;
        //            switch (nodeName)
        //            {
        //                case "p":
        //                    if (currentNode.FirstChild.Name.Equals("img"))
        //                    {
        //                        var uri = new Uri(currentNode.FirstChild.Attributes["src"].Value);
        //                        contents.Add(new Content(new BitmapImage(uri)));
        //                    }
        //                    else
        //                        contents.Add(new Content(currentNode.InnerText + "\n"));
        //                    break;
        //                case "#text":
        //                    var text = "";
        //                    while (!currentNode.Name.Equals("br") 
        //                        && !currentNode.Name.Equals("table") 
        //                        && !currentNode.Name.Equals("span")
        //                        && !currentNode.InnerText.Equals("/r/n"))
        //                    {
        //                        text += currentNode.InnerText;
        //                        currentNode = currentNode.NextSibling;
        //                        if (currentNode.InnerText.Contains("Article End"))
        //                            break;
        //                    }
        //                    if (!string.IsNullOrEmpty(text) && !text.Equals("/r/n"))
        //                        contents.Add(new Content(text));
        //                    break;
        //                case "a":
        //                    text = "";
        //                    while (!currentNode.Name.Equals("br"))
        //                    {
        //                        text += currentNode.InnerText;
        //                        currentNode = currentNode.NextSibling;
        //                    }
        //                    contents.Add(new Content(text));
        //                    break;
        //                case "em":
        //                    if (currentNode.FirstChild.Name.Equals("img"))
        //                    {
        //                        var uri = new Uri(currentNode.FirstChild.Attributes["src"].Value);
        //                        contents.Add(new Content(new BitmapImage(uri)));
        //                    }
        //                    else
        //                    {
        //                        text = "";
        //                        while (!currentNode.Name.Equals("br"))
        //                        {
        //                            text += currentNode.InnerText;
        //                            currentNode = currentNode.NextSibling;
        //                            if (currentNode.InnerText.Contains("Article End"))
        //                                break;
        //                        }
        //                        contents.Add(new Content(text));
        //                    }
        //                    break;
        //                //case "br":
        //                //    contents.Add(new Content("\n"));
        //                    //break;
        //            }
        //            if (currentNode.InnerText.Contains("Article End"))
        //                break;
        //            currentNode = currentNode.NextSibling;
        //            count++;
        //        }

        //        // get article author
        //        //loadedArticle.Author = htmlDoc.DocumentNode.SelectSingleNode("//h2[@class='author']").InnerText;

        //        this.Article = loadedArticle;
        //        this.Contents = contents;
        //        IsLoading = false;
        //    }
        //}
    }
}
