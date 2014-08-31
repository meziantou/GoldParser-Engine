namespace GoldParserEngine
{
    /// <summary>
    ///     The Rule class is used to represent the logical structures of the grammar.
    ///     Rules consist of a head containing a nonterminal followed by a series of both nonterminals and terminals.
    /// </summary>
    public class Production
    {
        private readonly SymbolList _handle;
        private readonly Symbol _head;

        private readonly short _tableIndex;

        internal Production(Symbol head, short tableIndex)
        {
            _head = head;
            _handle = new SymbolList();
            _tableIndex = tableIndex;
        }

        internal Production()
        {
            //Nothing
        }

        /// <summary>
        ///     Returns the head of the production.
        /// </summary>
        /// <returns></returns>
        public Symbol Head
        {
            get { return _head; }
        }

        /// <summary>
        ///     Returns the symbol list containing the handle (body) of the production.
        /// </summary>
        /// <returns></returns>
        public SymbolList Handle
        {
            get { return _handle; }
        }

        /// <summary>
        ///     Returns the index of the production in the Production Table.
        /// </summary>
        /// <returns></returns>
        public short TableIndex
        {
            get { return _tableIndex; }
        }

        public override string ToString()
        {
            return Text();
        }

        /// <summary>
        ///     Returns the production in BNF.
        /// </summary>
        /// <param name="alwaysDelimitTerminals"></param>
        /// <returns></returns>
        public string Text(bool alwaysDelimitTerminals = false)
        {
            return _head + " ::= " + _handle.ToString(" ", alwaysDelimitTerminals);
        }

        internal bool ContainsOneNonTerminal()
        {
            if (_handle.Count == 1)
            {
                if (_handle[0].Type == SymbolType.Nonterminal)
                {
                    return true;
                }
            }

            return false;
        }
    }
}