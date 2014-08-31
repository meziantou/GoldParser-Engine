using System;
using System.Runtime.Serialization;

namespace GoldParserEngine
{
    public class EngineException : Exception
    {
        public EngineException()
        {
        }

        public EngineException(string message) : base(message)
        {
        }

        public EngineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EngineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}