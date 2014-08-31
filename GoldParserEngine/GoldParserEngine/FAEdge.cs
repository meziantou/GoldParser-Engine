namespace GoldParserEngine
{
    /// <summary>
    ///     Each state in the Determinstic Finite Automata contains multiple edges which
    ///     link to other states in the automata.
    ///     This class is used to represent an edge.
    /// </summary>
    internal class FAEdge
    {
        public FAEdge(CharacterSet charSet, int target)
        {
            Characters = charSet;
            Target = target;
        }

        public FAEdge()
        {
        }

        /// <summary>
        ///     Characters to advance on
        /// </summary>
        /// <returns></returns>
        public CharacterSet Characters { get; private set; }

        /// <summary>
        ///     FAState
        /// </summary>
        /// <returns></returns>
        public int Target { get; private set; }
    }
}