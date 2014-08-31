namespace GoldParserEngine
{
    public class Position
    {
        internal Position()
        {
            Line = 0;
            Column = 0;
        }

        public int Line { get; set; }
        public int Column { get; set; }

        internal void Copy(Position position)
        {
            Column = position.Column;
            Line = position.Line;
        }
    }
}