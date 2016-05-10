using System;
using System.Collections.Generic;
using System.Linq;

namespace Debug_Library.WireProtocol
{
    public class StringEventArgs : EventArgs
    {
        public StringEventArgs(string _eventArgs)
        {
            this.EventText = _eventArgs;
        }
        public string EventText { get;  private set; } = null;
    }
}
