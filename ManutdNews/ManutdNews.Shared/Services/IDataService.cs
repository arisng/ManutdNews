using System;
using System.Linq;
using System.Text;
using ManutdNews.Models;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ManutdNews.Services
{
    public interface IDataService
    {
        void GetNewsDetails(Action<Article, Exception> callback);
        void GetNewsList(int pageNumber, Action<IEnumerable<Article>, Exception> callback);
        IEnumerable<Article> GetSampleNewsList();
    }
}
