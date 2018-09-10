using IBApi;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace IBSampleApp
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

        private void btnReqContracts_Click(object sender, RoutedEventArgs e)
        {
            TWS_RequestContractDetails("ES", "FUT", Exchanges.Globex);
            TWS_RequestContractDetails("NQ", "FUT", Exchanges.Globex);
        }

        private void btnReqData_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}