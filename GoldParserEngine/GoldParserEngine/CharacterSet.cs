using System.Collections.Generic;
using System.Linq;

namespace GoldParserEngine
{
    internal class CharacterSet : List<CharacterRange>
    {
        public bool Contains(int charCode)
        {
            return this.Any(_ => (charCode >= _.Start & charCode <= _.End));
        }
    }
}