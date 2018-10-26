using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IBSampleApp
{
    /// <summary>
    /// Convenience class used to eliminate the need for dependency properties
    /// Simply derive from Property updater, create a property as normal and use the UpdateProperty call in the settor.
    /// NOTE: there must be an UpdateProperty method for each type!
    /// </summary>
    public class PropertyUpdater : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // String version
        public void UpdateProperty(ref string property, string value, [CallerMemberName]string propertyName = "")
        {
            if (property != value)
            {
                property = value;
                NotifyPropertyChanged(propertyName);
            }
        }

        // double version
        public void UpdateProperty(ref double property, double value, [CallerMemberName]string propertyName="")
        {
            if (property != value)
            {
                property = value;
                NotifyPropertyChanged(propertyName);
            }
        }

    }
}
