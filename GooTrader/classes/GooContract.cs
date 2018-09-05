using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
namespace GooTrader
{
    public class GooContract : DependencyObject
    {
        #region Dependency Properties

        // Contract description
        #region Name
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(GooContract), new PropertyMetadata("None"));
        #endregion Name

        // Contract Symbol
        #region Symbol
        public string Symbol
        {
            get { return (string)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Symbol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol", typeof(string), typeof(GooContract), new PropertyMetadata(""));
        #endregion symbol

        // Contract expiration. For now we just pick the front month
        #region Expiration
        public string Expiration
        {
            get { return (string)GetValue(ExpirationProperty); }
            set { SetValue(ExpirationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Expiration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpirationProperty =
            DependencyProperty.Register("Expiration", typeof(string), typeof(GooContract), new PropertyMetadata(null));
        #endregion

        #endregion Dependency Properties
    }
}
