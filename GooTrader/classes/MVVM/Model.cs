using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace GooTrader
{
    /// <summary>
    /// Model is the domain object for program data. It knows nothing of the ViewModel or the View.
    /// To incorporate data from the model, a mirror object should be created in the View model.
    /// </summary>
    public class Model
    {
        public Dictionary<string,GooContract> contracts { get; set; }

        #region Constructor
        public Model()
        {
            contracts = new Dictionary<string, GooContract>();
        }
        #endregion Constructor
    }
}
