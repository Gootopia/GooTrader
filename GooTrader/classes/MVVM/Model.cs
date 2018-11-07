using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System;

namespace IBSampleApp
{
    /// <summary>
    /// Model is the domain object for program data. It knows nothing of the ViewModel or the View.
    /// To incorporate data from the model, a mirror object should be created in the View model which pulls out the relevant data.
    /// </summary>
    public class Model
    {
        // Contracts list.
        public Dictionary<string, GooContract> Contracts = new Dictionary<string, GooContract>();

        #region Constructor
        public Model()
        {
        }

        #endregion Constructor
    }
}
