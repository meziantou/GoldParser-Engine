namespace GoldParserEngine
{
    /// <summary>
    ///     This class is used to store of the nonterminals used by the Deterministic
    ///     Finite Automata (DFA) and LALR Parser. Symbols can be either
    ///     terminals (which represent a class of tokens - such as identifiers) or
    ///     nonterminals (which represent the rules and structures of the grammar).
    ///     Terminal symbols fall into several catagories for use by the GOLD Parser
    ///     Engine which are enumerated below.
    /// </summary>
    public class Symbol
    {
        private readonly string _name;
        private readonly short _tableIndex;
        private readonly SymbolType _type;

        internal Group Group;

        internal Symbol(string name, SymbolType type, short tableIndex)
        {
            _name = name;
            _type = type;
            _tableIndex = tableIndex;
        }

        /// <summary>
        ///     Returns the type of the symbol.
        /// </summary>
        /// <returns></returns>
        public SymbolType Type
        {
            get { return _type; }
        }

        /// <summary>
        ///     Returns the index of the symbol in the Symbol Table.
        /// </summary>
        /// <returns></returns>
        public short TableIndex
        {
            get { return _tableIndex; }
        }

        /// <summary>
        ///     Returns the name of the symbol.
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get { return _name; }
        }

        private string FormatLiteral(string source, bool forceDelimit)
        {
            if (source == "'")
            {
                return "''";
            }

            for (int i = 0; i < source.Length && !forceDelimit; i++)
            {
                char ch = source[i];
                forceDelimit = !(char.IsLetter(ch) || ch == '.' || ch == '_' || ch == '-');
            }

            if (forceDelimit)
            {
                return "'" + source + "'";
            }

            return source;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool alwaysDelimitTerminals)
        {
            switch (_type)
            {
                case SymbolType.Nonterminal:
                    return "<" + Name + ">";
                case SymbolType.Terminal:
                    return FormatLiteral(Name, alwaysDelimitTerminals);
                default:
                    return "(" + Name + ")";
            }
        }
    }
}