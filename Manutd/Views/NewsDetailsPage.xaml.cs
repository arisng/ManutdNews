using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Manutd.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace Manutd.Views
{
    public partial class NewsDetailsPage : PhoneApplicationPage
    {
        NewsDetailsViewModel viewModel;

        public NewsDetailsPage()
        {
            InitializeComponent();
            viewModel = (NewsDetailsViewModel)DataContext;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //viewModel.StartScrapingArticle(parameter);
            //string url = "";
            //if (NavigationContext.QueryString.TryGetValue("url", out url))
            //{
            //    viewModel.StartScrapingArticle(url);
            //}
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            viewModel.Article = new Models.Article();
            if (viewModel.Contents != null)
                viewModel.Contents.Clear();
        }

        //protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        //{
        //    base.OnBackKeyPress(e);
        //    if (NavigationService.CanGoBack)
        //        NavigationService.GoBack();
        //}
    }
}