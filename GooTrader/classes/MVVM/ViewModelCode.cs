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

        private void btnReqContracts_Click(object sender, RoutedEventArgs e)
        {
            TWS.RequestContractDetails("ES", "FUT", TWSInfo.TWS_Exchanges.Globex);
            TWS.RequestContractDetails("NQ", "FUT", TWSInfo.TWS_Exchanges.Globex);
        }

        private void btnReqData_Click(object sender, RoutedEventArgs e)
        {
            FSM_DownloadHistoricalData fsd = new FSM_DownloadHistoricalData();
            fsd.FireEvent(FSM_DownloadHistoricalData.Events.GotContractDetails);
            fsd.FireEvent(FSM_DownloadHistoricalData.Events.GotHeadTimeStamp);
        }
    }
}