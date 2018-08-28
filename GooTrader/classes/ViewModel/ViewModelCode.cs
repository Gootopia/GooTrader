using IBApi;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace GooTrader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            TWS_Connect();
        }
    }
}