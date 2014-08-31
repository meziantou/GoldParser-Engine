using System.Collections.Generic;

namespace GoldParserEngine
{
    internal class CharacterSetList : List<CharacterSet>
    {
        public CharacterSetList()
        {
        }

        internal CharacterSetList(int size) : base(size)
        {
            //Increase the size of the array to Size empty elements.
            for (int n = 0; n < size; n++)
            {
                Add(null);
            }
        }
    }
}