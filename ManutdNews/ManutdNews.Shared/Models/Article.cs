using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace ManutdNews.Models
{
    public class Article : ViewModelBase
    {
        public Article() 
        {
            Contents = new ObservableCollection<Content>();
        }

        public Article(string title, string summary, ObservableCollection<Content> contents, string author, string pubDateString, BitmapImage image, string articleUrl)
        {
            this.title = title;
            this.summary = summary;
            this.contents = contents;
            this.author = author;
            this.pubDateString = pubDateString;
            this.image = image;
            this.articleUrl = articleUrl;
        }

        private string title;

        public string Title
        {
            get { return title; }
            set 
            {
                if (this.title == value) return;
                title = value;
                this.RaisePropertyChanged(() => Title);
            }
        }

        private string summary;

        public string Summary
        {
            get { return summary; }
            set
            {
                if (this.summary == value) return;
                summary = value;
                this.RaisePropertyChanged(() => Summary);
            }
        }

        private ObservableCollection<Content> contents;

        public ObservableCollection<Content> Contents
        {
            get { return contents; }
            set
            {
                if (this.contents == value) return;
                contents = value;
                this.RaisePropertyChanged(() => Contents);
            }
        }

        private string author;

        public string Author
        {
            get { return author; }
            set
            {
                if (this.author == value) return;
                author = value;
                this.RaisePropertyChanged(() => Author);
            }
        }

        private DateTime pubdate;

        public DateTime PubDate
        {
            get { return pubdate; }
            set
            {
                if (this.pubdate == value) return;
                pubdate = value;
                this.RaisePropertyChanged(() => PubDate);
            }
        }

        private string pubDateString;
        public string PubDateString
        {
            get { return pubDateString; }
            set
            {
                if (this.pubDateString == value) return;
                pubDateString = value;
                this.RaisePropertyChanged(() => PubDateString);
            }
        }

        private BitmapImage image;

        public BitmapImage Image
        {
            get { return image; }
            set
            {
                if (this.image == value) return;
                image = value;
                this.RaisePropertyChanged(() => Image);
            }
        }

        private string articleUrl;

        public string ArticleUrl
        {
            get { return articleUrl; }
            set
            {
                if (this.articleUrl == value) return;
                articleUrl = value;
                this.RaisePropertyChanged(() => ArticleUrl);
            }
        }

        private string articleSource;

        public string ArticleSource
        {
            get { return this.articleSource; }
            set
            {
                if (value == null) return;
                this.articleSource = value;
            }
        }
    }
}
