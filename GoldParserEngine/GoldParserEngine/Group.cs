namespace GoldParserEngine
{
    internal class Group
    {
        public enum AdvanceMode
        {
            Token = 0,
            Character = 1
        }

        public enum EndingMode
        {
            Open = 0,
            Closed = 1
        }

        public Group()
        {
            Advance = AdvanceMode.Character;
            Ending = EndingMode.Closed;
            Nesting = new IntegerList();
        }

        public short TableIndex { get; set; }
        public string Name { get; set; }
        public Symbol Container { get; set; }
        public Symbol Start { get; set; }
        public Symbol End { get; set; }
        public AdvanceMode Advance { get; set; }
        public EndingMode Ending { get; set; }
        public IntegerList Nesting { get; private set; }
    }
}