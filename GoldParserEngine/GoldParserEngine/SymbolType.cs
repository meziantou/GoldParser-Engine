namespace GoldParserEngine
{
    public enum SymbolType
    {
        Nonterminal = 0,
        Terminal = 1,
        Noise = 2,
        End = 3,
        GroupStart = 4,
        GroupEnd = 5,
        //Note: There is no value 6. CommentLine was deprecated.
        Error = 7
    }
}