using System.Collections.Generic;

namespace GoldParserEngine
{
    internal class FAStateList : List<FAState>
    {
        public short InitialState;

        //public Symbol ErrorSymbol;
        public FAStateList()
        {
            InitialState = 0;
            //ErrorSymbol = null;
        }

        internal FAStateList(int size) : base(size)
        {
            InitialState = 0;
            //ErrorSymbol = null;

            for (int n = 0; n < size; n++)
            {
                Add(null);
            }
        }
    }
}