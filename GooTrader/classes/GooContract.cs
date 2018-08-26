using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
namespace GooTrader
{
    public class GooContract : DependencyObject
    {
        public GooContract(string name)
        {
            Name = name;
        }

        #region Dependency Properties
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(GooContract), new PropertyMetadata("None"));
        #endregion Dependency Properties
    }
}
