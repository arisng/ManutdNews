using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ManutdNews.Models;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;

namespace ManutdNews.Services
{
    public class DesignDataService : IDataService
    {
        public void GetNewsDetails(Action<Article, Exception> callback)
        {
            //var dateString = "05/09/2013 16:04:00";
            //var pubDate = DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
            var uri = new Uri("http://u.goal.com/309800/309882_thumb.jpg", UriKind.Absolute);
            ObservableCollection<Content> contents = new ObservableCollection<Content> { new Content { } };
            var item = new Article("Van Persie tán dương Moyes",
                "Robin van Persie vừa dành những lời có cánh cho ông thầy hiện tại ở Manchester United, David Moyes.",
                contents,
                "Mark Froggatt",
                "",
                new BitmapImage(uri),
                "");
            callback(item, null);
        }

        public void GetNewsList(int pageNumber, Action<IEnumerable<Article>, Exception> callback)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<Article> GetSampleNewsList()
        {
            var newsItems = from n in Enumerable.Range(1, 10)
                            select new Article
                            {
                                Title = "Đã Thua Trận, United Lại Mất Người",
                                Summary = "HLV Louis van Gaal xác nhận Robin van Persie phải rời sân sớm trong trận thua ngày hôm qua trước Southampton vì dính chấn thương mắt cá.",
                                PubDateString = "Ngày cập nhật: 12/01/2015",
                                Image = new BitmapImage(new Uri("/Aset/article-pic1", UriKind.Relative))
                            };
            return newsItems;
        }
    }
}