namespace GoldParserEngine
{
    /// <summary>
    ///     While the Symbol represents a class of terminals and nonterminals, the
    ///     Token represents an individual piece of information.
    ///     Ideally, the token would inherit directly from the Symbol Class, but do to
    ///     the fact that Visual Basic 5/6 does not support this aspect of Object Oriented
    ///     Programming, a Symbol is created as a member and its methods are mimicked.
    /// </summary>
    public class Token
    {
        private readonly Position _position = new Position();
        private Symbol _parent;

        internal Token()
        {
            _parent = null;
            Data = null;
            State = 0;
        }

        public Token(Symbol parent, object data)
        {
            _parent = parent;
            Data = data;
            State = 0;
        }

        /// <summary>
        ///     Returns the line/column position where the token was read.
        /// </summary>
        /// <returns></returns>
        public Position Position
        {
            get { return _position; }
        }

        /// <summary>
        ///     Returns/sets the object associated with the token.
        /// </summary>
        /// <returns></returns>
        public object Data { get; set; }

        internal short State { get; set; }

        /// <summary>
        ///     Returns the parent symbol of the token.
        /// </summary>
        /// <returns></returns>
        public Symbol Parent
        {
            get { return _parent; }
            internal set { _parent = value; }
        }

        /// <summary>
        ///     "Returns the symbol type associated with this token."
        /// </summary>
        /// <returns></returns>
        public SymbolType Type
        {
            get { return _parent.Type; }
        }

        internal Group Group
        {
            get { return _parent.Group; }
        }
    }
}