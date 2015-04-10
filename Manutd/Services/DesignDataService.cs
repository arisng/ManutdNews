using System;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Manutd.Models;
using System.Collections.Generic;

namespace Manutd.Services
{
    public class DesignDataService : IDataService
    {


        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to create design time data

            var item = new DataItem("Welcome to MVVM Light [design]", "");
            callback(item, null);
        }

        public void GetArticleItem(Action<Article, Exception> callback)
        {
            //var dateString = "05/09/2013 16:04:00";
            //var pubDate = DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
            var uri = new Uri("http://u.goal.com/309800/309882_thumb.jpg", UriKind.Absolute);
            ObservableCollection<Content> contents = new ObservableCollection<Content> { new Content { } };
            //content.Add("Text", "Robin van Persie vừa dành những lời có cánh cho ông thầy hiện tại ở Manchester United, David Moyes. Trên tờ De Telegraaf của Hà Lan, Robin van Persie nhận xét: Thật tuyệt khi được làm việc với HLV mới David Moyes. Ông ấy có phong cách riêng, có phương pháp riêng và tôi rất thích điều đó. Chúng tôi đã làm việc rất chăm chỉ ngày qua ngày kể từ khi chúng tôi bắt đầu tour du đấu châu Á - Australia. Kết quả là những trận đấu tốt. Nhưng vẫn luôn có áp lực tại đội bóng. Tôi hài lòng với phong cách của Moyes. Ông ấy tự đề ra các giáo án tập luyện, ông ấy cũng rất gần gũi với cầu thủ, ban huấn luyện, chuẩn bị rất chu đáo cho đội bóng trước mỗi trận đấu. Moyes giúp Manchester United luôn trong trạng thái sẵn sàng. Và chúng tôi có may mắn là nhà đương kim vô địch. Đó là động lực giúp chúng tôi tiếp tục tiến lên.");
            var item = new Article("Van Persie tán dương Moyes",
                "Robin van Persie vừa dành những lời có cánh cho ông thầy hiện tại ở Manchester United, David Moyes.",
                contents,
                "Mark Froggatt",
                "",
                new BitmapImage(uri),
                "");
            callback(item, null);
        }

        public void GetArticleCollection(Action<ObservableCollection<Article>, Exception> callback)
        {

        }
    }
}