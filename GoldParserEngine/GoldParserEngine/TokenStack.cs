using System.Collections.Generic;

namespace GoldParserEngine
{
    /// <summary>
    ///     This class is used by the GOLDParser class to store tokens during parsing.
    ///     In particular, this class is used the the LALR(1) state machine.
    /// </summary>
    internal class TokenStack : Stack<Token>
    {
    }
}