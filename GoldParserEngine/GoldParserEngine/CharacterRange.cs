namespace GoldParserEngine
{
    internal class CharacterRange
    {
        public CharacterRange(ushort start, ushort end)
        {
            Start = start;
            End = end;
        }

        public ushort Start { get; private set; }
        public ushort End { get; private set; }
    }
}