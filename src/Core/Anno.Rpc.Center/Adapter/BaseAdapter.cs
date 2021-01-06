using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Adapter
{
    abstract class BaseAdapter
    {
        internal abstract string Invoke(Dictionary<string, string> command);
    }
}
