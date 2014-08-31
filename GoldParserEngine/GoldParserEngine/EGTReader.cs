using System.IO;

namespace GoldParserEngine
{
    internal class EGTReader
    {
        public enum EntryType : byte
        {
            Empty = 69, //E
            UInt16 = 73, //I - Unsigned, 2 byte
            String = 83, //S - Unicode format
            Boolean = 66, //B - 1 Byte, ValueExpression is 0 or 1
            Byte = 98, //b
            Error = 0
        }

        private const byte RecordContentMulti = 77;
        private int _entriesRead;
        private int _entryCount;
        private string _fileHeader;

        private BinaryReader _reader;

        private string GetErrorMessage(EntryType type, BinaryReader reader)
        {
            return "Type mismatch in file. Read '" + type + "' at " + reader.BaseStream.Position;
        }

        public bool RecordComplete()
        {
            return _entriesRead >= _entryCount;
        }

        public int EntryCount()
        {
            return _entryCount;
        }

        public bool EndOfFile()
        {
            return _reader.BaseStream.Position == _reader.BaseStream.Length;
        }

        public string Header()
        {
            return _fileHeader;
        }

        public void Open(BinaryReader reader)
        {
            _reader = reader;

            _entryCount = 0;
            _entriesRead = 0;
            _fileHeader = RawReadCString();
        }

        public void Open(string path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    Open(binaryReader);
                }
            }
        }

        public Entry RetrieveEntry()
        {
            var result = new Entry();

            if (RecordComplete())
            {
                result.Type = EntryType.Empty;
                result.Value = string.Empty;
            }
            else
            {
                _entriesRead += 1;
                var type = (EntryType) _reader.ReadByte(); //Entry Type Prefix
                result.Type = type;

                switch (type)
                {
                    case EntryType.Empty:
                        result.Value = string.Empty;
                        break;
                    case EntryType.Boolean:
                        byte b = _reader.ReadByte();
                        result.Value = (b == 1);
                        break;
                    case EntryType.UInt16:
                        result.Value = RawReadUInt16();
                        break;
                    case EntryType.String:
                        result.Value = RawReadCString();
                        break;
                    case EntryType.Byte:
                        result.Value = _reader.ReadByte();
                        break;
                    default:
                        result.Type = EntryType.Error;
                        result.Value = string.Empty;
                        break;
                }
            }

            return result;
        }

        private ushort RawReadUInt16()
        {
            //Read a uint in little endian. This is the format already used
            //by the .NET BinaryReader. However, it is good to specificially
            //define this given byte order can change depending on platform.

            int b0 = _reader.ReadByte();
            int b1 = _reader.ReadByte();

            return (ushort) ((b1 << 8) + b0);
        }

        private string RawReadCString()
        {
            string text = string.Empty;
            bool done = false;

            while (!done)
            {
                ushort char16 = RawReadUInt16();
                if (char16 == 0)
                {
                    done = true;
                }
                else
                {
                    text += (char) char16;
                }
            }

            return text;
        }

        public string RetrieveString()
        {
            Entry e = RetrieveEntry();
            if (e.Type == EntryType.String)
            {
                return (string) e.Value;
            }

            throw new EngineException(GetErrorMessage(e.Type, _reader));
        }

        public int RetrieveInt16()
        {
            Entry e = RetrieveEntry();
            if (e.Type == EntryType.UInt16)
            {
                return (ushort) e.Value;
            }

            throw new EngineException(GetErrorMessage(e.Type, _reader));
        }

        public bool RetrieveBoolean()
        {
            Entry e = RetrieveEntry();
            if (e.Type == EntryType.Boolean)
            {
                return (bool) e.Value;
            }

            throw new EngineException(GetErrorMessage(e.Type, _reader));
        }

        public byte RetrieveByte()
        {
            Entry e = RetrieveEntry();
            if (e.Type == EntryType.Byte)
            {
                return (byte) e.Value;
            }

            throw new EngineException(GetErrorMessage(e.Type, _reader));
        }

        public bool GetNextRecord()
        {
            //==== Finish current record
            while (_entriesRead < _entryCount)
            {
                RetrieveEntry();
            }

            //==== Start next record
            byte id = _reader.ReadByte();
            if (id == RecordContentMulti)
            {
                _entryCount = RawReadUInt16();
                _entriesRead = 0;
                return true;
            }

            return false;
        }

        public class Entry
        {
            public Entry()
            {
                Type = EntryType.Empty;
                Value = string.Empty;
            }

            public Entry(EntryType type, object value)
            {
                Type = type;
                Value = value;
            }

            public EntryType Type { get; set; }
            public object Value { get; set; }
        }
    }
}