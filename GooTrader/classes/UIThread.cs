using System;
using System.Windows;
using System.Windows.Threading;

namespace IBSampleApp
{
    /// <summary>
    /// Cosmetic class to make things more readable in the code
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
