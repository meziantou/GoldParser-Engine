namespace GoldParserEngine
{
    public enum ParseMessage
    {
        /// <summary>
        /// A new token is read
        /// </summary>
        TokenRead = 0,
        /// <summary>
        /// A production is reduced
        /// </summary>
        Reduction = 1,
        /// <summary>
        /// Grammar complete
        /// </summary>
        Accept = 2,
        /// <summary>
        /// The tables are not loaded
        /// </summary>
        NotLoadedError = 3,
        /// <summary>
        /// Token not recognized
        /// </summary>
        LexicalError = 4,
        /// <summary>
        /// Token is not expected
        /// </summary>
        SyntaxError = 5,
        /// <summary>
        /// Reached the end of the file inside a block
        /// </summary>
        GroupError = 6,
        /// <summary>
        /// Something is wrong, very wrong
        /// </summary>
        InternalError = 7
    }
}