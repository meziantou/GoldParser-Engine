namespace GoldParserEngine
{
    public class GrammarProperties
    {
        private const int PropertyCount = 8;
        private readonly string[] _properties = new string[PropertyCount + 1];

        internal GrammarProperties()
        {
            for (int n = 0; n < PropertyCount; n++)
            {
                _properties[n] = string.Empty;
            }
        }

        public string Name
        {
            get { return _properties[(int) PropertyIndex.Name]; }
        }

        public string Version
        {
            get { return _properties[(int) PropertyIndex.Version]; }
        }

        public string Author
        {
            get { return _properties[(int) PropertyIndex.Author]; }
        }

        public string About
        {
            get { return _properties[(int) PropertyIndex.About]; }
        }

        public string CharacterSet
        {
            get { return _properties[(int) PropertyIndex.CharacterSet]; }
        }

        public string CharacterMapping
        {
            get { return _properties[(int) PropertyIndex.CharacterMapping]; }
        }

        public string GeneratedBy
        {
            get { return _properties[(int) PropertyIndex.GeneratedBy]; }
        }

        public string GeneratedDate
        {
            get { return _properties[(int) PropertyIndex.GeneratedDate]; }
        }

        internal void SetValue(int index, string value)
        {
            if (index >= 0 & index < PropertyCount)
            {
                _properties[index] = value;
            }
        }

        private enum PropertyIndex
        {
            Name = 0,
            Version = 1,
            Author = 2,
            About = 3,
            CharacterSet = 4,
            CharacterMapping = 5,
            GeneratedBy = 6,
            GeneratedDate = 7
        }
    }
}