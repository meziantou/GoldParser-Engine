namespace GoldParserEngine
{
    /// <summary>
    ///     Represents a state in the Deterministic Finite Automata which is used by the tokenizer.
    /// </summary>
    internal class FAState
    {
        public FAState(Symbol accept)
        {
            Accept = accept;
            Edges = new FAEdgeList();
        }

        public FAState()
        {
            Accept = null;
            Edges = new FAEdgeList();
        }

        public FAEdgeList Edges { get; private set; }

        public Symbol Accept { get; private set; }
    }
}