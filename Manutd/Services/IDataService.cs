using System;
using System.Linq;
using System.Text;
using Manutd.Models;
using System.Collections.ObjectModel;

namespace Manutd.Services
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
        void GetArticleItem(Action<Article, Exception> callback);
        void GetArticleCollection(Action<ObservableCollection<Article>, Exception> callback);
    }
}
