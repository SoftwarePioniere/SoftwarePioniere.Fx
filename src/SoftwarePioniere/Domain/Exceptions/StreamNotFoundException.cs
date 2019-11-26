using System;

namespace SoftwarePioniere.Domain.Exceptions
{
    public class StreamNotFoundException : Exception
    {
        public string StreamName { get; }

        public StreamNotFoundException(string streamName)
        {
            StreamName = streamName;
        }
    }
}