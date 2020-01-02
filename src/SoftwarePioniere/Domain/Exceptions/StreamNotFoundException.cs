namespace SoftwarePioniere.Domain.Exceptions
{
    public class StreamNotFoundException : SopiException
    {
        public string StreamName { get; }

        public StreamNotFoundException(string streamName)
        {
            StreamName = streamName;
        }
    }
}