namespace GoldParserEngine
{
    /// <summary>
    ///     This class is used by the engine to hold a reduced rule.
    ///     Rather the contain a list of Symbols, a reduction contains a list of Tokens corresponding to the the rule it
    ///     represents.
    ///     This class is important since it is used to store the actual source program parsed by the Engine.
    /// </summary>
    public class Reduction : TokenList
    {
        internal Reduction(int size)
        {
            for (int n = 0; n < size; n++)
            {
                base.Add(null);
            }
        }

        /// <summary>
        ///     Returns the parent production.
        /// </summary>
        /// <returns></returns>
        public Production Parent { get; internal set; }

        /// <summary>
        ///     Returns/sets any additional user-defined data to this object.
        /// </summary>
        /// <returns></returns>
        public object Tag { get; set; }

        public object GetData(int index)
        {
            return this[index].Data;
        }

        public void SetData(int index, object value)
        {
            this[index].Data = value;
        }
    }
}