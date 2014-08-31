using System;
using System.Collections.Generic;

namespace GoldParserEngine
{
    internal class LRState : List<LRAction>
    {
        public LRAction this[Symbol symbol]
        {
            get
            {
                int index = IndexOf(symbol);
                if (index >= 0)
                {
                    return base[index];
                }

                return null;
            }
        }

        public int IndexOf(Symbol item)
        {
            if (item == null) throw new ArgumentNullException("item");

            for (int i = 0; i < Count; i++)
            {
                if (item.Equals(this[i].Symbol))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}