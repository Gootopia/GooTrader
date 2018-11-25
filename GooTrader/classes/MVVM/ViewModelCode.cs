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
    /// Interaction logic for MainWindow.xaml UI Events
    /// </summary>
    public partial class MainWindow : Window
    {
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            TWS.Connect();
        }


        GooContract sp500, nq100;
        
        private void btnReqContracts_Click(object sender, RoutedEventArgs e)
        {
            sp500 = TWS.GetContractDetails("ES", "FUT", TWSInfo.TWS_Exchanges.Globex);
            nq100 = TWS.GetContractDetails("NQ", "FUT", TWSInfo.TWS_Exchanges.Globex);
        }

        private void btnReqData_Click(object sender, RoutedEventArgs e)
        {
            TWS.GetHistoricalData(sp500);
        }
    }
}