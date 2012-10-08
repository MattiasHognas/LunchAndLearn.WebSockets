using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSockets.UI
{
    public class Pair<T1, T2>
    {
        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
        public T1 First { get; set; }
        public T2 Second { get; set; }
    }
}
