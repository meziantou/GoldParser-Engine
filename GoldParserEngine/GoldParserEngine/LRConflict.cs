namespace GoldParserEngine
{
    internal enum LRConflict
    {
        ShiftShift = 1,
        //Never happens
        ShiftReduce = 2,
        ReduceReduce = 3,
        AcceptReduce = 4,
        //Never happens with this implementation
        None = 5
    }
}