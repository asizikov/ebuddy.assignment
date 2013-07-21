﻿namespace eBuddy.Assignment.Navigation
{
    public interface INavigationService
    {
        void Navigate(string pageName, string parameterQueue = null);
        void GoBack();
    }
}