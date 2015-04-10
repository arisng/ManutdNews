using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Manutd.ViewModel;

namespace Manutd
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ScrollViewer sv = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void myScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            SetScrollViewer();
        }

        private void SetScrollViewer()
        {
            sv = (ScrollViewer)FindElementRecursive(myListBox, typeof(ScrollViewer));

            // Visual States are always on the first child of the control template 
            FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
            if (element != null)
            {
                VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");

                if (vgroup != null)
                {
                    vgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(vgroup_CurrentStateChanging);
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

            IList groups = VisualStateManager.GetVisualStateGroups(element);
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