using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace Manutd.Models
{
    public class DataItem : ViewModelBase
    {
        private string title;
        private string content;

        public DataItem() { }

        public DataItem(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                if (this.title == value)
                    return;
                this.title = value;
                this.RaisePropertyChanged(() => this.Title);
            }
        }

        public string Content
        {
            get { return this.content; }
            set
            {
                if (this.content == value)
                    return;
                this.content = value;
                this.RaisePropertyChanged(() => this.Content);
            }
        }
    }
}
