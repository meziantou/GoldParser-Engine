using System.Collections.Generic;

namespace GoldParserEngine
{
    internal class LRStateList : List<LRState>
    {
        public short InitialState;

        public LRStateList()
        {
            InitialState = 0;
        }

        internal LRStateList(int size) : base(size)
        {
            InitialState = 0;

            for (int n = 0; n < size; n++)
            {
                Add(null);
            }
        }
    }
}