namespace GoldParserEngine
{
    internal class LRAction
    {
        public LRAction(Symbol theSymbol, LRActionType type, short value)
        {
            Symbol = theSymbol;
            Type = type;
            Value = value;
        }

        public Symbol Symbol { get; private set; }
        public LRActionType Type { get; private set; }
        public short Value { get; private set; }
    }
}