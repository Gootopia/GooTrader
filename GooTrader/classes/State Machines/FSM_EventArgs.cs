using System;

namespace IBSampleApp
{
    /// <summary>
    /// Class with various available event arguments used by FSM.
    /// These allow data to be passed to states when firing events
    /// </summary>
    public static class FSM_EventArgs
    {
        /// <summary>
        /// Event that applies to a specific contract.
        /// Optional Payload varies, depending on the event (tick data, order, etc. etc).
        /// </summary>
        public class GooContract_With_Payload
        {
            // Contract used by the event
            public GooContract Contract { get; set; }
            
            // Object payload. Can be anything. The event will convert
            public object Payload { get; set; }

            // Can remove default null if we want events to be explicit that there is no payload
            public GooContract_With_Payload(GooContract c, object p = null)
            {
                Contract = c;
                Payload = p;
            }
        }

        /// <summary>
        /// Event which simply contains a data payload. These are typically non-specific to a contract (system stuff)
        /// such as connection related events.
        /// </summary>
        public class Payload_Only
        {
            // Object payload. Can be anything. The event will convert
            public object Payload { get; set; }

            // Can remove default null if we want events to be explicit that there is no payload
            public Payload_Only(object p=null)
            {
                Payload = p;
            }
        }
    }
}
