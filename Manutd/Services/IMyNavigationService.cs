using System;
using System.Collections.Generic;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace Manutd.Services
{
    public interface IMyNavigationService
    {
        bool CanGoBack { get; }
        bool Navigate(Uri source);
        void GoBack();
        event NavigatedEventHandler Navigated;
        event NavigatingCancelEventHandler Navigating;
        event EventHandler<ObscuredEventArgs> Obscured;
        bool RecoveredFromTombstoning { get; set; }
        void UpdateTombstonedPageTracking(Uri pageUri);
        bool DoesPageNeedtoRecoverFromTombstoning(Uri pageUri);
    }
}
