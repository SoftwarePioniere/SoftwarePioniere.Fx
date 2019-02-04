using System.Collections.Generic;

namespace SoftwarePioniere.Messaging
{
    public interface ICommand : IMessage
    {
        /// <summary>
        /// Die EventVersion des Original Objekts
        /// </summary>     
        int OriginalVersion { get; }

        string ObjectType { get; }

        string ObjectId { get; }

        IDictionary<string, string> Properties { get; }

    }
}