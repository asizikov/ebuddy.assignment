using System;
using JetBrains.Annotations;
using Microsoft.Phone.Controls;
using eBuddy.Assignment.Utils;

namespace eBuddy.Assignment.Navigation
{
    public sealed class NavigationService : INavigationService
    {
        [NotNull]
        private readonly PhoneApplicationFrame _rootFrame;

        public NavigationService(PhoneApplicationFrame rootFrame)
        {
            rootFrame.ThrowIfNull("rootFrame");
            _rootFrame = rootFrame;
        }

        public void Navigate(string pageName, string parameterQueue = null)
        {
            var uri = string.IsNullOrEmpty(parameterQueue)
                          ? pageName
                          : pageName + parameterQueue;
            _rootFrame.Navigate(new Uri(uri, UriKind.Relative));
        }

        public void GoBack()
        {
            _rootFrame.GoBack();
        }
    }
}

