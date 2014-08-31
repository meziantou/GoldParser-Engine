using System.Collections.Generic;

namespace GoldParserEngine
{
    internal class GroupList : List<Group>
    {
        public GroupList()
        {
        }

        internal GroupList(int size)
        {
            for (int n = 0; n < size; n++)
            {
                Add(null);
            }
        }
    }
}