namespace GoldParserEngine
{
    internal enum LRActionType
    {
        /// <summary>
        /// Shift a symbol and goto a state
        /// </summary>
        Shift = 1,
        /// <summary>
        /// Reduce by a specified rule
        /// </summary>
        Reduce = 2,
        /// <summary>
        /// Goto to a state on reduction
        /// </summary>
        Goto = 3,
        /// <summary>
        /// Input successfully parsed
        /// </summary>
        Accept = 4,
        /// <summary>
        /// Programmars see this often!
        /// </summary>
        Error = 5 
    }
}