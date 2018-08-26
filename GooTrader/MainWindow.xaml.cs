using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace GooTrader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<GooContract> contracts = new ObservableCollection<GooContract>();

        public GooContract c1 = new GooContract("SP500");
        public GooContract c2 = new GooContract("NQ");
        public IBClient ib = new IBClient();

        public MessageLogger gLog = new MessageLogger();
         public MainWindow()
        {
            InitializeComponent();

            contracts.Add(c1);
            contracts.Add(c2);
     
            viewContracts.DataContext = contracts;

            viewLog.ItemsSource = gLog;
         }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            c1.Name = c1.Name + "-*";
            gLog.LogMessage("MyMessage");
            //ib.clientSocket.eConnect("127.0.0.1", 7497, 0);
        }
    }
}
