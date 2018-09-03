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

        private void btnReqData_Click(object sender, RoutedEventArgs e)
        {
            IBApi.Contract fut_sp500 = new Contract();
            fut_sp500.Symbol = "ES";
            fut_sp500.SecType = "FUT";
            fut_sp500.Exchange = Exchanges.Globex;
            ib.ClientSocket.reqContractDetails(ib.NextOrderId, fut_sp500);

            IBApi.Contract fut_nq = new Contract();
            fut_nq.Symbol = "NQ";
            fut_nq.SecType = "FUT";
            fut_nq.Exchange = Exchanges.Globex;
            ib.ClientSocket.reqContractDetails(ib.NextOrderId, fut_nq);

        }
    }
}