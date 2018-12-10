using System;

namespace IBSampleApp
{
    /// <summary>
    /// Finite State Machine event packet.
    /// </summary>
    public class FSM_EventArgs
    {
        // Contract used by the event
        public GooContract Contract { get; set; }
        // Object payload. Can be anything. The event will convert
        public object Payload { get; set; }

        public FSM_EventArgs(GooContract c, object p = null)
        {
            Contract = c;
            Payload = p;
        }
    }
}
