using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace ManutdNews.Models
{
    public class Content : ViewModelBase
    {
        public Content()
        {
            text = "Example";
            image = new BitmapImage();
        }

        public Content(string text)
        {
            this.text = text;
        }

        public Content(BitmapImage image)
        {
            this.image = image;
        }

        public Content(string t, BitmapImage i)
        {
            text = t;
            image = i;
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
        private string text;

        public string Text
        {
            get { return text; }
            set 
            {
                if (this.text == value) return;
                text = value;
                this.RaisePropertyChanged(() => Text);
            }
        }
    }
}
