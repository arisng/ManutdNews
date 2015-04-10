using System;
using System.Collections.Generic;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace Manutd.Services
{
    /// <summary>
    /// This class provides both a wrapper over navigation related methods provided by the 
    /// PhoneApplicationFrame, and tombstoning support. The RecoveredFromTombstoning property is expected to be 
    /// set based on the value of e.IsApplicationInstancePreserved in the Application_Activated event handler (App.xaml.cs).
    /// However, since this value is scoped to the application, this class also determines which pages
    /// were tombstoned so that they can properly recover from tombstoning.
    /// </summary>
    public class MyNavigationService : IMyNavigationService
    {
        private readonly PhoneApplicationFrame frame;
        private Dictionary<string, bool> tombstonedPages;

        //Contructor
        public MyNavigationService(PhoneApplicationFrame frame)
        {
            this.frame = frame;
            this.frame.Navigated += frame_Navigated;
            this.frame.Navigating += frame_Navigating;
            this.frame.Obscured += frame_Obscured;
            this.RecoveredFromTombstoning = false;
        }

        //Simple wrapper over PhoneApplicationFrame.CanGoBack.
        public bool CanGoBack
        {
            get { return this.frame.CanGoBack; }
        }

        ///This value is expected to be set based on the value of e.IsApplicationInstancePreserved 
        ///in the Application_Activated event handler (App.xaml.cs).
        public bool RecoveredFromTombstoning { get; set; }

        /// This method determines if a given page needs to recover from tombstoning. 
        /// This is used in the base ViewModel class in order to determine whether or not to call the 
        /// ViewModel.OnPageResumeFromTombstoning method.
        /// This method fails fast if the application did not resume from tombstoning 
        /// (see the RecoveredFromTombstoning property).
        /// 
        /// It is assumed that we can determine the list of pages that were tombstoned by taking the
        /// first page/view model that was instantiated due to a tombstoning recovery, and then examining
        /// the backstack. This method populates a dictionary with the pages that were tombstoned and 
        /// uses this dictionary to track whether the base ViewModel class called the OnPageResumedFromTombstoning
        /// method for each tombstoned page.
        public bool DoesPageNeedtoRecoverFromTombstoning(Uri pageUri)
        {
            if (!RecoveredFromTombstoning) return false;

            if (tombstonedPages == null)
            {
                tombstonedPages = new Dictionary<string, bool>();
                tombstonedPages.Add(pageUri.ToString(), true);
                foreach (var journalEntry in frame.BackStack)
                {
                    tombstonedPages.Add(journalEntry.Source.ToString(), true);
                }
                return true;
            }

            if (tombstonedPages.ContainsKey(pageUri.ToString()))
            {
                return tombstonedPages[pageUri.ToString()];
            }
            return false;
        }

        public void UpdateTombstonedPageTracking(Uri pageUri)
        {
            tombstonedPages[pageUri.ToString()] = false;
        }

        //Simple wrapper over PhoneApplicationFrame.Navigate.
        public bool Navigate(Uri source)
        {
            return this.frame.Navigate(source);
        }

        //Simple wrapper over PhoneApplicationFrame.GoBack.
        public void GoBack()
        {
            this.frame.GoBack();
        }

        public event NavigatedEventHandler Navigated;

        void frame_Navigated(object sender, NavigationEventArgs e)
        {
            var handler = this.Navigated;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public event NavigatingCancelEventHandler Navigating;

        void frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var handler = this.Navigating;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public event EventHandler<ObscuredEventArgs> Obscured;

        void frame_Obscured(object sender, ObscuredEventArgs e)
        {
            var handler = this.Obscured;
            if (handler != null)
            {
                handler(sender, e);
            }
        }
    }
}
