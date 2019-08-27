using System;
using System.Windows;
using GooTrader;

namespace IBSampleApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml UI Events
    /// </summary>
    public partial class MainWindow : Window
    {
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            TWS.FSM.Connection.FireEvent(FSM_TwsConnectivity.Events.Connect);
        }

        GooContract sp500, nq100;
        
        private void btnReqContracts_Click(object sender, RoutedEventArgs e)
        {
            sp500 = TWS.GetContractDetails("ES", "FUT", TWSInfo.Exchanges.Globex);
            nq100 = TWS.GetContractDetails("NQ", "FUT", TWSInfo.Exchanges.Globex);
        }

        private void btnReqData_Click(object sender, RoutedEventArgs e)
        {
            TWS.GetHistoricalData(sp500);
            TWS.GetHistoricalData(nq100);
        }
    }
}