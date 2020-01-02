using System;
using System.Runtime.Serialization;

namespace SoftwarePioniere
{
    [Serializable]
    public class SopiException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SopiException()
        {
        }

        public SopiException(string message) : base(message)
        {
        }

        public SopiException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SopiException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}