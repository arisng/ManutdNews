using ManutdNews.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace ManutdNews.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ScrollViewer sv = null;

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void myScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            SetScrollViewer();
        }

        private void SetScrollViewer()
        {
            sv = (ScrollViewer)FindElementRecursive(newsListView, typeof(ScrollViewer));

            // Visual States are always on the first child of the control template 
            FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
            if (element != null)
            {
                VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");

                if (vgroup != null)
                {
                    vgroup.CurrentStateChanging += new VisualStateChangedEventHandler(vgroup_CurrentStateChanging);
                }
            }
        }

        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            var mainViewModel = (MainViewModel)DataContext;
            //if (e.NewState.Name == "CompressionTop")
            //{

            //}

            if (e.NewState.Name == "CompressionBottom")
            {
                mainViewModel.LoadMoreCommand.Execute(null);
            }
            //if (e.NewState.Name == "NoVerticalCompression")
            //{

            //}
        }

        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList<VisualStateGroup> groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        return element as UIElement;
                    }
                    else
                    {
                        returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType);
                    }
                }
            }
            return returnElement;
        }
    }
}
