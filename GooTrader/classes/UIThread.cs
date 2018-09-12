using System;
using System.Windows;
using System.Windows.Threading;

namespace IBSampleApp
{
    /// <summary>
    /// Used to execute UI operations outside the UI thread. Only needed for DependencyObjects.
    /// Properties that use the INotifyPropertyChanged (i.e: PropertyUpdater) can just assign as normal.
    /// </summary>
    static class UIThread
    {
        /// <summary>
        /// Update(func)
        /// Convenience method for methods outside the UI thread to update items in the UI thread.
        /// Note that this specifically refers to items in the Viewmodel that are bound to the UI and
        /// are updated by events in the TWS thread
        /// </summary>
        /// <param name="func"></param>
        static public void Update(Action func)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, func);
        }
    }
}
